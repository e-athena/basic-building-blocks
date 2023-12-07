// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Serialization;

namespace Athena.Infrastructure.FreeSql.CAPs.Extends;

internal class FreeSqlMonitoringApi : IMonitoringApi
{
    private readonly string _pubName;
    private readonly string _recName;
    private readonly ISerializer _serializer;
    private readonly IFreeSql _freeSql;

    public FreeSqlMonitoringApi(
        IStorageInitializer initializer,
        ISerializer serializer, IFreeSql freeSql)
    {
        _serializer = serializer;
        _freeSql = freeSql;
        _pubName = initializer.GetPublishedTableName();
        _recName = initializer.GetReceivedTableName();
    }

    Task<IDictionary<DateTime, int>> IMonitoringApi.HourlySucceededJobs(MessageType type)
    {
        var tableName = type == MessageType.Publish ? _pubName : _recName;
        return GetHourlyTimelineStats(tableName, nameof(StatusName.Succeeded));
    }

    Task<IDictionary<DateTime, int>> IMonitoringApi.HourlyFailedJobs(MessageType type)
    {
        var tableName = type == MessageType.Publish ? _pubName : _recName;
        return GetHourlyTimelineStats(tableName, nameof(StatusName.Failed));
    }

    ValueTask<int> IMonitoringApi.ReceivedSucceededCount()
    {
        return GetNumberOfMessage(_recName, nameof(StatusName.Succeeded));
    }

    public async Task<PagedQueryResult<MessageDto>> GetMessagesAsync(MessageQueryDto queryDto)
    {
        var result = await _freeSql.Select<Published>()
            .AsTable((_, _) => queryDto.MessageType == MessageType.Publish ? _pubName : _recName)
            .WhereIf(!string.IsNullOrEmpty(queryDto.StatusName),
                p => p.StatusName.ToLower() == queryDto.StatusName!.ToLower())
            .WhereIf(!string.IsNullOrEmpty(queryDto.Name), p => p.Name == queryDto.Name)
            .WhereIf(!string.IsNullOrEmpty(queryDto.Group), p => p.Group == queryDto.Group)
            .WhereIf(!string.IsNullOrEmpty(queryDto.Content),
                p => p.Content.Contains(queryDto.Content!))
            .OrderByDescending(p => p.Added)
            .ToPagingAsync(new GetPagingRequestBase(queryDto.CurrentPage + 1, queryDto.PageSize))
            .ConfigureAwait(false);
        return new PagedQueryResult<MessageDto>
        {
            Items = (result.Items ?? new List<Published>()).Select(p => new MessageDto
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
            Totals = result.TotalItems
        };
    }

    ValueTask<int> IMonitoringApi.PublishedFailedCount()
    {
        return GetNumberOfMessage(_pubName, nameof(StatusName.Failed));
    }

    ValueTask<int> IMonitoringApi.PublishedSucceededCount()
    {
        return GetNumberOfMessage(_pubName, nameof(StatusName.Succeeded));
    }

    ValueTask<int> IMonitoringApi.ReceivedFailedCount()
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

    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var statistics = new StatisticsDto
        {
            PublishedSucceeded = (int) await _freeSql.Queryable<Published>()
                .AsTable((_, _) => _pubName)
                .Where(p => p.StatusName == "Succeeded")
                .CountAsync(),
            ReceivedSucceeded = (int) await _freeSql.Queryable<Received>()
                .AsTable((_, _) => _recName)
                .Where(p => p.StatusName == "Succeeded")
                .CountAsync(),
            PublishedFailed = (int) await _freeSql.Queryable<Published>()
                .AsTable((_, _) => _pubName)
                .Where(p => p.StatusName == "Failed")
                .CountAsync(),
            ReceivedFailed = (int) await _freeSql.Queryable<Received>()
                .AsTable((_, _) => _recName)
                .Where(p => p.StatusName == "Failed")
                .CountAsync(),
            PublishedDelayed = (int) await _freeSql.Queryable<Published>()
                .AsTable((_, _) => _pubName)
                .Where(p => p.StatusName == "Delayed")
                .CountAsync()
        };

        return statistics;
    }

    public StatisticsDto GetStatistics()
    {
        var pubCount = _freeSql.Select<Published>()
            .AsTable((_, _) => _pubName)
            .GroupBy(p => p.StatusName)
            .ToList(p => new
            {
                p.Key,
                Count = p.Count()
            });
        var recCount = _freeSql.Select<Received>()
            .AsTable((_, _) => _recName)
            .GroupBy(p => p.StatusName)
            .ToList(p => new
            {
                p.Key,
                Count = p.Count()
            });

        // 统计数据

        return new StatisticsDto
        {
            PublishedSucceeded = pubCount.FirstOrDefault(p => p.Key == nameof(StatusName.Succeeded))?.Count ?? 0,
            ReceivedSucceeded = recCount.FirstOrDefault(p => p.Key == nameof(StatusName.Succeeded))?.Count ?? 0,
            PublishedFailed = pubCount.FirstOrDefault(p => p.Key == nameof(StatusName.Failed))?.Count ?? 0,
            ReceivedFailed = recCount.FirstOrDefault(p => p.Key == nameof(StatusName.Failed))?.Count ?? 0
        };
    }

    private async ValueTask<int> GetNumberOfMessage(string tableName, string statusName)
    {
        var count = await _freeSql.Select<MessageDto>()
            .AsTable((_, _) => tableName)
            .Where(p => p.StatusName == statusName)
            .CountAsync();

        return (int) count;
    }

    private Task<IDictionary<DateTime, int>> GetHourlyTimelineStats(string tableName, string statusName)
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

    private async Task<IDictionary<DateTime, int>> GetTimelineStats(
        string tableName,
        string statusName,
        IDictionary<string, DateTime> keyMaps)
    {
        // 翻译上面的SQL转成_freeSql的写法
        var res = await _freeSql.Select<Published>()
            .AsTable((_, _) => tableName)
            .Where(p => p.StatusName == statusName)
            .GroupBy(p => p.Added.ToString("yyyy-MM-dd-HH"))
            .WithTempQuery(p => new
            {
                p.Key,
                Count = p.Count()
            })
            .Where(p => Convert.ToDateTime(p.Key) >= Convert.ToDateTime(keyMaps.Keys.Min()))
            .Where(p => Convert.ToDateTime(p.Key) <= Convert.ToDateTime(keyMaps.Keys.Max()))
            .ToListAsync(p => new
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
        var result = await _freeSql.Queryable<Published>()
            .AsTable((_, _) => tableName)
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