// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends;

internal class SqlSugarMonitoringApi : IMonitoringApi
{
    private readonly string _pubName;
    private readonly string _recName;
    private readonly ISerializer _serializer;
    private readonly ISqlSugarClient _sqlSugarClient;

    public SqlSugarMonitoringApi(
        IStorageInitializer initializer,
        ISerializer serializer, ISqlSugarClient sqlSugarClient)
    {
        _serializer = serializer;
        _sqlSugarClient = sqlSugarClient;
        _pubName = initializer.GetPublishedTableName();
        _recName = initializer.GetReceivedTableName();
    }

    IDictionary<DateTime, int> IMonitoringApi.HourlySucceededJobs(MessageType type)
    {
        var tableName = type == MessageType.Publish ? _pubName : _recName;
        return GetHourlyTimelineStats(tableName, nameof(StatusName.Succeeded));
    }

    IDictionary<DateTime, int> IMonitoringApi.HourlyFailedJobs(MessageType type)
    {
        var tableName = type == MessageType.Publish ? _pubName : _recName;
        return GetHourlyTimelineStats(tableName, nameof(StatusName.Failed));
    }

    int IMonitoringApi.ReceivedSucceededCount()
    {
        return GetNumberOfMessage(_recName, nameof(StatusName.Succeeded));
    }

    public PagedQueryResult<MessageDto> Messages(MessageQueryDto queryDto)
    {
        var query = _sqlSugarClient.Queryable<Published>()
            .AS(queryDto.MessageType == MessageType.Publish ? _pubName : _recName)
            .WhereIF(!string.IsNullOrEmpty(queryDto.StatusName),
                p => p.StatusName.ToLower() == queryDto.StatusName!.ToLower())
            .WhereIF(!string.IsNullOrEmpty(queryDto.Name), p => p.Name == queryDto.Name)
            .WhereIF(!string.IsNullOrEmpty(queryDto.Group), p => p.Group == queryDto.Group)
            .WhereIF(!string.IsNullOrEmpty(queryDto.Content), p => p.Content.Contains(queryDto.Content!));
        // var count = query.Count();
        var count = 0;
        var result = query
            .ToPageList(queryDto.CurrentPage, queryDto.PageSize, totalNumber: ref count)
            .ToList();
        return new PagedQueryResult<MessageDto>
        {
            Items = result.Select(p => new MessageDto
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                Group = p.Group,
                Content = p.Content,
                StatusName = p.StatusName,
                Retries = p.Retries,
                Added = p.Added,
                ExpiresAt = p.ExpiresAt,
                Version = p.Version
            }).ToList(),
            PageIndex = queryDto.CurrentPage,
            PageSize = queryDto.PageSize,
            Totals = count
        };
    }

    int IMonitoringApi.PublishedFailedCount()
    {
        return GetNumberOfMessage(_pubName, nameof(StatusName.Failed));
    }

    int IMonitoringApi.PublishedSucceededCount()
    {
        return GetNumberOfMessage(_pubName, nameof(StatusName.Succeeded));
    }

    int IMonitoringApi.ReceivedFailedCount()
    {
        return GetNumberOfMessage(_recName, nameof(StatusName.Failed));
    }

    public async Task<MediumMessage?> GetPublishedMessageAsync(long id)
    {
        return await GetMessageAsync(_pubName, id).ConfigureAwait(false);
    }

    public async Task<MediumMessage?> GetReceivedMessageAsync(long id)
    {
        return await GetMessageAsync(_recName, id).ConfigureAwait(false);
    }

    public StatisticsDto GetStatistics()
    {
        var pubCount = _sqlSugarClient.Queryable<Published>()
            .AS(_pubName)
            .GroupBy(p => p.StatusName)
            .Select(p => new
            {
                Key = p.StatusName,
                Count = SqlFunc.AggregateCount(p.StatusName)
            })
            .ToList();
        var recCount = _sqlSugarClient.Queryable<Received>()
            .AS(_recName)
            .GroupBy(p => p.StatusName)
            .Select(p => new
            {
                Key = p.StatusName,
                Count = SqlFunc.AggregateCount(p.StatusName)
            })
            .ToList();

        // 统计数据

        return new StatisticsDto
        {
            PublishedSucceeded = pubCount.FirstOrDefault(p => p.Key == nameof(StatusName.Succeeded))?.Count ?? 0,
            ReceivedSucceeded = recCount.FirstOrDefault(p => p.Key == nameof(StatusName.Succeeded))?.Count ?? 0,
            PublishedFailed = pubCount.FirstOrDefault(p => p.Key == nameof(StatusName.Failed))?.Count ?? 0,
            ReceivedFailed = recCount.FirstOrDefault(p => p.Key == nameof(StatusName.Failed))?.Count ?? 0
        };
    }

    private int GetNumberOfMessage(string tableName, string statusName)
    {
        var count = _sqlSugarClient.Queryable<MessageDto>()
            .AS(tableName)
            .Where(p => p.StatusName == statusName)
            .Count();

        return count;
    }

    private Dictionary<DateTime, int> GetHourlyTimelineStats(string tableName, string statusName)
    {
        var endDate = DateTime.Now;
        var dates = new List<DateTime>();
        for (var i = 0; i < 24; i++)
        {
            dates.Add(endDate);
            endDate = endDate.AddHours(-1);
        }

        var keyMaps = dates.ToDictionary(x => x.ToString("yyyy-MM-dd-HH"), x => x);

        return GetTimelineStats(tableName, statusName, keyMaps);
    }

    private Dictionary<DateTime, int> GetTimelineStats(
        string tableName,
        string statusName,
        IDictionary<string, DateTime> keyMaps)
    {
        // 翻译上面的SQL转成_sqlSugarClient的写法
        var res = _sqlSugarClient.Queryable<Published>()
            .AS(tableName)
            .Where(p => p.StatusName == statusName)
            .GroupBy(p => p.Added.ToString("yyyy-MM-dd-HH"))
            .Select(p => new
            {
                Key = p.Added.ToString("yyyy-MM-dd-HH"),
                Count = SqlFunc.AggregateCount(p.Added)
            })
            .MergeTable()
            .Where(p => Convert.ToDateTime(p.Key) >= Convert.ToDateTime(keyMaps.Keys.Min()))
            .Where(p => Convert.ToDateTime(p.Key) <= Convert.ToDateTime(keyMaps.Keys.Max()))
            .ToList(p => new
            {
                p.Key,
                p.Count
            });

        var valuesMap = res.ToDictionary(p => p.Key, p => p.Count);

        foreach (var key in keyMaps.Keys)
        {
            valuesMap.TryAdd(key, 0);
        }

        var result = new Dictionary<DateTime, int>();
        for (var i = 0; i < keyMaps.Count; i++)
        {
            var value = valuesMap[keyMaps.ElementAt(i).Key];
            result.Add(keyMaps.ElementAt(i).Value, value);
        }

        return result;
    }

    private async Task<MediumMessage?> GetMessageAsync(string tableName, long id)
    {
        var result = await _sqlSugarClient.Queryable<Published>()
            .AS(tableName)
            .Where(p => p.Id == id)
            .FirstAsync().ConfigureAwait(false);

        if (result == null)
        {
            return null;
        }

        return new MediumMessage
        {
            DbId = result.Id.ToString(),
            Origin = _serializer.Deserialize(result.Content)!,
            Content = result.Content,
            Added = result.Added,
            ExpiresAt = result.ExpiresAt,
            Retries = result.Retries
        };
    }
}