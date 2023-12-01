// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data.Common;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.Options;
using Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class SqlSugarDataStorage : IDataStorage
{
    private readonly IOptionsMonitor<CapOptions> _capOptions;
    private readonly IStorageInitializer _initializer;
    private readonly string _pubName;
    private readonly string _recName;
    private readonly string _lockName;
    private readonly ISerializer _serializer;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ISnowflakeId _snowflakeId;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="capOptions"></param>
    /// <param name="initializer"></param>
    /// <param name="serializer"></param>
    /// <param name="sqlSugarClient"></param>
    /// <param name="snowflakeId"></param>
    public SqlSugarDataStorage(IOptionsMonitor<CapOptions> capOptions,
        IStorageInitializer initializer,
        ISerializer serializer,
        ISqlSugarClient sqlSugarClient, ISnowflakeId snowflakeId)
    {
        _capOptions = capOptions;
        _initializer = initializer;
        _serializer = serializer;
        _sqlSugarClient = sqlSugarClient;
        _snowflakeId = snowflakeId;
        _pubName = initializer.GetPublishedTableName();
        _recName = initializer.GetReceivedTableName();
        _lockName = initializer.GetLockTableName();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ttl"></param>
    /// <param name="instance"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<bool> AcquireLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = new())
    {
        var opResult = await _sqlSugarClient.Updateable<Lock>()
            .AS(_lockName)
            .SetColumns(a => a.Instance, instance)
            .SetColumns(a => a.LastLockTime, DateTime.Now)
            .Where(a => a.Key == key && a.LastLockTime < DateTime.Now.Subtract(ttl))
            .ExecuteCommandAsync(token)
            .ConfigureAwait(false);

        return opResult > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="instance"></param>
    /// <param name="token"></param>
    public async Task ReleaseLockAsync(string key, string instance, CancellationToken token = new())
    {
        await _sqlSugarClient.Updateable<Lock>()
            .AS(_lockName)
            .SetColumns(a => a.Instance, "")
            .SetColumns(a => a.LastLockTime, DateTime.MinValue)
            .Where(a => a.Key == key && a.Instance == instance)
            .ExecuteCommandAsync(token)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="ttl"></param>
    /// <param name="instance"></param>
    /// <param name="token"></param>
    public async Task RenewLockAsync(string key, TimeSpan ttl, string instance,
        CancellationToken token = new())
    {
        await _sqlSugarClient.Updateable<Lock>()
            .AS(_lockName)
            .SetColumns(a => a.LastLockTime, DateTime.Now.AddSeconds(ttl.TotalSeconds))
            .Where(a => a.Key == key && a.Instance == instance)
            .ExecuteCommandAsync(token)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ChangePublishStateToDelayedAsync(string[] ids)
    {
        await _sqlSugarClient.Updateable<Published>()
            .AS(_pubName)
            .SetColumns(a => a.StatusName, StatusName.Delayed.ToString())
            .Where(a => ids.Contains(a.Id.ToString()))
            .ExecuteCommandAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="state"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public Task ChangePublishStateAsync(MediumMessage message, StatusName state, object? transaction = null)
    {
        return ChangeMessageStateAsync(_pubName, message, state, transaction);
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
    public async Task<MediumMessage> StoreMessageAsync(string name, Message content, object? transaction = null)
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
            StatusName = StatusName.Scheduled.ToString(),
            Version = _capOptions.CurrentValue.Version
        };
        if (transaction is DbTransaction dbTransaction)
        {
            _sqlSugarClient.Ado.Transaction = dbTransaction;
        }

        await _sqlSugarClient.Insertable(dto)
            .AS(_pubName)
            .ExecuteCommandAsync();

        return message;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="content"></param>
    public async Task StoreReceivedExceptionMessageAsync(string name, string group, string content)
    {
        var dto = new Received
        {
            Id = long.Parse(_snowflakeId.NextId().ToString()),
            Name = name,
            Group = group,
            Content = content,
            Retries = _capOptions.CurrentValue.FailedRetryCount,
            Added = DateTime.Now,
            ExpiresAt = DateTime.Now.AddSeconds(_capOptions.CurrentValue.FailedMessageExpiredAfter),
            StatusName = nameof(StatusName.Failed),
            Version = _capOptions.CurrentValue.Version
        };

        await _sqlSugarClient.Insertable(dto).AS(_recName).ExecuteCommandAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<MediumMessage> StoreReceivedMessageAsync(string name, string group, Message content)
    {
        var mdMessage = new MediumMessage
        {
            DbId = _snowflakeId.NextId().ToString(),
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
            Version = _capOptions.CurrentValue.Version
        };
        await _sqlSugarClient.Insertable(rec).AS(_recName).ExecuteCommandAsync();
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
                (p.StatusName == StatusName.Succeeded.ToString() || p.StatusName == StatusName.Failed.ToString())
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
        var fourMinAgo = DateTime.Now.AddMinutes(-4);

        var result = await _sqlSugarClient.Queryable<Published>()
            .AS(_pubName)
            .Where(p => p.Retries < _capOptions.CurrentValue.FailedRetryCount)
            .Where(p => p.Version == _capOptions.CurrentValue.Version)
            .Where(p => p.Added < fourMinAgo)
            .Where(p =>
                p.StatusName == StatusName.Failed.ToString() ||
                p.StatusName == StatusName.Scheduled.ToString()
            )
            .Take(200)
            .ToListAsync();

        if (result == null)
        {
            return new List<MediumMessage>();
        }

        return result.Select(p => new MediumMessage
        {
            DbId = p.Id.ToString(),
            Origin = _serializer.Deserialize(p.Content)!,
            Retries = p.Retries,
            Added = p.Added
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scheduleTask"></param>
    /// <param name="token"></param>
    public async Task ScheduleMessagesOfDelayedAsync(Func<object, IEnumerable<MediumMessage>, Task> scheduleTask,
        CancellationToken token = new())
    {
        var result = await _sqlSugarClient.Queryable<Published>()
            .AS(_pubName)
            .Where(p => p.Version == _capOptions.CurrentValue.Version)
            .Where(p =>
                (p.ExpiresAt < DateTime.Now.AddMinutes(2) && p.StatusName == StatusName.Delayed.ToString()) ||
                (p.ExpiresAt < DateTime.Now.AddMinutes(-1) && p.StatusName == StatusName.Queued.ToString()))
            .ToListAsync(token)
            .ConfigureAwait(false);

        if (result == null)
        {
            await scheduleTask(token, new List<MediumMessage>());
            return;
        }

        var messageList = result
            .Select(p => new MediumMessage
            {
                DbId = p.Id.ToString(),
                Origin = _serializer.Deserialize(p.Content)!,
                Retries = p.Retries,
                Added = p.Added,
                ExpiresAt = p.ExpiresAt
            });
        await scheduleTask(token, messageList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<MediumMessage>> GetReceivedMessagesOfNeedRetry()
    {
        var fourMinAgo = DateTime.Now.AddMinutes(-4);

        var result = await _sqlSugarClient.Queryable<Received>()
            .AS(_recName)
            .Where(p => p.Retries < _capOptions.CurrentValue.FailedRetryCount)
            .Where(p => p.Version == _capOptions.CurrentValue.Version)
            .Where(p => p.Added < fourMinAgo)
            .Where(p =>
                p.StatusName == StatusName.Failed.ToString() ||
                p.StatusName == StatusName.Scheduled.ToString()
            )
            .Take(200)
            .ToListAsync();

        if (result == null)
        {
            return new List<MediumMessage>();
        }

        return result.Select(p => new MediumMessage
        {
            DbId = p.Id.ToString(),
            Origin = _serializer.Deserialize(p.Content)!,
            Retries = p.Retries,
            Added = p.Added
        });
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
            _sqlSugarClient.Ado.Transaction = dbTransaction;
            await _sqlSugarClient.Updateable<Published>()
                .AS(tableName)
                .SetColumns(a => a.Content, _serializer.Serialize(message.Origin))
                .SetColumns(a => a.Retries, message.Retries)
                .SetColumns(a => a.ExpiresAt, message.ExpiresAt)
                .SetColumns(a => a.StatusName, state.ToString())
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
                .SetColumns(a => a.StatusName, state.ToString())
                .Where(a => a.Id == dbId)
                .ExecuteCommandAsync()
                .ConfigureAwait(false);
        }
    }
}