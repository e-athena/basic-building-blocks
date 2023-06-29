// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data.Common;
using Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.Options;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class SqlSugarDataStorage : IDataStorage
{
    private readonly IOptions<CapOptions> _capOptions;
    private readonly IStorageInitializer _initializer;
    private readonly string _pubName;
    private readonly string _recName;
    private readonly ISerializer _serializer;
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="capOptions"></param>
    /// <param name="initializer"></param>
    /// <param name="serializer"></param>
    /// <param name="sqlSugarClient"></param>
    public SqlSugarDataStorage(IOptions<CapOptions> capOptions,
        IStorageInitializer initializer,
        ISerializer serializer,
        ISqlSugarClient sqlSugarClient)
    {
        _capOptions = capOptions;
        _initializer = initializer;
        _serializer = serializer;
        _sqlSugarClient = sqlSugarClient;
        _pubName = initializer.GetPublishedTableName();
        _recName = initializer.GetReceivedTableName();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="state"></param>
    public async Task ChangePublishStateAsync(MediumMessage message, StatusName state)
    {
        await ChangeMessageStateAsync(_pubName, message, state).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="state"></param>
    public async Task ChangeReceiveStateAsync(MediumMessage message, StatusName state)
    {
        await ChangeMessageStateAsync(_recName, message, state).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="content"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public MediumMessage StoreMessage(string name, Message content, object? transaction = null)
    {
        var message = new MediumMessage
        {
            DbId = content.GetId(),
            Origin = content,
            Content = _serializer.Serialize(content),
            Added = DateTime.Now,
            ExpiresAt = null,
            Retries = 0
        };
        var dto = new Published
        {
            Id = long.Parse(message.DbId),
            Name = name,
            Content = message.Content,
            Retries = message.Retries,
            Added = message.Added,
            ExpiresAt = message.ExpiresAt,
            StatusName = StatusName.Scheduled.ToString("G"),
            Version = _capOptions.Value.Version
        };
        if (transaction is DbTransaction dbTransaction)
        {
            _sqlSugarClient.Insertable(dto)
                .AS(_pubName)
                .ExecuteCommand();
        }
        else
        {
            _sqlSugarClient.Insertable(dto)
                .AS(_pubName)
                .ExecuteCommand();
        }

        return message;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="content"></param>
    public void StoreReceivedExceptionMessage(string name, string group, string content)
    {
        var dto = new Received
        {
            Id = long.Parse(SnowflakeId.Default().NextId().ToString()),
            Name = name,
            Group = group,
            Content = content,
            Retries = _capOptions.Value.FailedRetryCount,
            Added = DateTime.Now,
            ExpiresAt = DateTime.Now.AddSeconds(_capOptions.Value.FailedMessageExpiredAfter),
            StatusName = nameof(StatusName.Failed),
            Version = _capOptions.Value.Version
        };

        _sqlSugarClient.Insertable(dto).AS(_recName).ExecuteCommand();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public MediumMessage StoreReceivedMessage(string name, string group, Message content)
    {
        var mdMessage = new MediumMessage
        {
            DbId = SnowflakeId.Default().NextId().ToString(),
            Origin = content,
            Added = DateTime.Now,
            ExpiresAt = null,
            Retries = 0
        };
        var rec = new Received
        {
            Id = long.Parse(mdMessage.DbId),
            Name = name,
            Group = group,
            Content = _serializer.Serialize(mdMessage.Origin),
            Retries = mdMessage.Retries,
            Added = mdMessage.Added,
            ExpiresAt = mdMessage.ExpiresAt ?? DateTime.MinValue,
            StatusName = nameof(StatusName.Scheduled),
            Version = _capOptions.Value.Version
        };
        _sqlSugarClient.Insertable(rec).AS(_recName).ExecuteCommand();
        return mdMessage;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="table"></param>
    /// <param name="timeout"></param>
    /// <param name="batchCount"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<int> DeleteExpiresAsync(string table, DateTime timeout, int batchCount = 1000,
        CancellationToken token = default)
    {
        return await _sqlSugarClient.Deleteable<Received>(p =>
                p.ExpiresAt < timeout &&
                (p.StatusName == StatusName.Succeeded.ToString("G") || p.StatusName == StatusName.Failed.ToString("G"))
            )
            .AS(table)
            .ExecuteCommandAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry()
    {
        return await GetMessagesOfNeedRetryAsync(_pubName).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry()
    {
        return await GetMessagesOfNeedRetryAsync(_recName).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IMonitoringApi GetMonitoringApi()
    {
        return new SqlSugarMonitoringApi(_initializer, _serializer, _sqlSugarClient);
    }

    private async Task ChangeMessageStateAsync(string tableName, MediumMessage message, StatusName state,
        object? transaction = null)
    {
        var dbId = long.Parse(message.DbId);
        if (transaction is DbTransaction dbTransaction)
        {
            await _sqlSugarClient.Updateable<Published>()
                .AS(tableName)
                .SetColumns(a => a.Content, _serializer.Serialize(message.Origin))
                .SetColumns(a => a.Retries, message.Retries)
                .SetColumns(a => a.ExpiresAt, message.ExpiresAt)
                .SetColumns(a => a.StatusName, state.ToString("G"))
                .Where(a => a.Id == dbId)
                .ExecuteCommandAsync()
                .ConfigureAwait(false);
        }
        else
        {
            await _sqlSugarClient.Updateable<Published>()
                .AS(tableName)
                .SetColumns(a => a.Content, _serializer.Serialize(message.Origin))
                .SetColumns(a => a.Retries, message.Retries)
                .SetColumns(a => a.ExpiresAt, message.ExpiresAt)
                .SetColumns(a => a.StatusName, state.ToString("G"))
                .Where(a => a.Id == dbId)
                .ExecuteCommandAsync()
                .ConfigureAwait(false);
        }
    }

    private async Task<IEnumerable<MediumMessage>> GetMessagesOfNeedRetryAsync(string tableName)
    {
        var fourMinAgo = DateTime.Now.AddMinutes(-4);

        var result = await _sqlSugarClient.Queryable<Received>()
            .AS(tableName)
            .Where(p => p.Retries < _capOptions.Value.FailedRetryCount)
            .Where(p => p.Version == _capOptions.Value.Version)
            .Where(p => p.Added < fourMinAgo)
            .Where(p => p.StatusName == StatusName.Failed.ToString("G") ||
                        p.StatusName == StatusName.Scheduled.ToString("G"))
            .Take(200)
            .ToListAsync()
            .ConfigureAwait(false);

        return result.Select(p => new MediumMessage
        {
            DbId = p.Id.ToString(),
            Origin = _serializer.Deserialize(p.Content)!,
            Retries = p.Retries,
            Added = p.Added
        });
    }
}