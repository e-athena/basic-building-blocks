// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data.Common;
using Athena.Infrastructure.FreeSql.CAPs.Extends.Models;
using DotNetCore.CAP.Internal;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Monitoring;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Serialization;
using Microsoft.Extensions.Options;

namespace Athena.Infrastructure.FreeSql.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class FreeSqlDataStorage : IDataStorage
{
    private readonly IOptions<CapOptions> _capOptions;
    private readonly IStorageInitializer _initializer;
    private readonly string _pubName;
    private readonly string _recName;
    private readonly string _lockName;
    private readonly ISerializer _serializer;
    private readonly IFreeSql _freeSql;
    private readonly ISnowflakeId _snowflakeId;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="capOptions"></param>
    /// <param name="initializer"></param>
    /// <param name="serializer"></param>
    /// <param name="freeSql"></param>
    /// <param name="snowflakeId"></param>
    public FreeSqlDataStorage(IOptions<CapOptions> capOptions,
        IStorageInitializer initializer,
        ISerializer serializer,
        IFreeSql freeSql, ISnowflakeId snowflakeId)
    {
        _capOptions = capOptions;
        _initializer = initializer;
        _serializer = serializer;
        _freeSql = freeSql;
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
        var opResult = await _freeSql.Update<Lock>()
            .AsTable(_lockName)
            .Set(a => a.Instance, instance)
            .Set(a => a.LastLockTime, DateTime.Now)
            .Where(a => a.Key == key && a.LastLockTime < DateTime.Now.Subtract(ttl))
            .ExecuteAffrowsAsync(token)
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
        await _freeSql.Update<Lock>()
            .AsTable(_lockName)
            .Set(a => a.Instance, "")
            .Set(a => a.LastLockTime, DateTime.MinValue)
            .Where(a => a.Key == key && a.Instance == instance)
            .ExecuteAffrowsAsync(token)
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
        await _freeSql.Update<Lock>()
            .AsTable(_lockName)
            .Set(a => a.LastLockTime, DateTime.Now.AddSeconds(ttl.TotalSeconds))
            .Where(a => a.Key == key && a.Instance == instance)
            .ExecuteAffrowsAsync(token)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public Task ChangePublishStateToDelayedAsync(string[] ids)
    {
        return _freeSql.Update<Published>()
            .AsTable(_pubName)
            .Set(a => a.StatusName, StatusName.Delayed.ToString())
            .Where(a => ids.Contains(a.Id.ToString()))
            .ExecuteAffrowsAsync();
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
    /// <param name="group"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task StoreReceivedExceptionMessageAsync(string name, string group, string content)
    {
        return _freeSql.Insert<Received>()
            .AsTable(_recName)
            .AppendData(new Received
            {
                Id = long.Parse(_snowflakeId.NextId().ToString()),
                Name = name,
                Group = group,
                Content = content,
                Retries = _capOptions.Value.FailedRetryCount,
                Added = DateTime.Now,
                ExpiresAt = DateTime.Now.AddSeconds(_capOptions.Value.FailedMessageExpiredAfter),
                StatusName = nameof(StatusName.Failed),
                Version = _capOptions.Value.Version
            })
            .ExecuteAffrowsAsync();
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
        if (transaction is DbTransaction dbTransaction)
        {
            _freeSql.Insert<Published>()
                .WithTransaction(dbTransaction)
                .AsTable(_pubName)
                .AppendData(new Published
                {
                    Id = long.Parse(message.DbId),
                    Name = name,
                    Content = message.Content,
                    Retries = message.Retries,
                    Added = message.Added,
                    ExpiresAt = message.ExpiresAt,
                    StatusName = StatusName.Scheduled.ToString(),
                    Version = _capOptions.Value.Version
                })
                .ExecuteAffrows();
        }
        else
        {
            _freeSql.Insert<Published>()
                .AsTable(_pubName)
                .AppendData(new Published
                {
                    Id = long.Parse(message.DbId),
                    Name = name,
                    Content = message.Content,
                    Retries = message.Retries,
                    Added = message.Added,
                    ExpiresAt = message.ExpiresAt,
                    StatusName = StatusName.Scheduled.ToString(),
                    Version = _capOptions.Value.Version
                })
                .ExecuteAffrows();
        }

        return message;
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
        if (transaction is DbTransaction dbTransaction)
        {
            await _freeSql.Insert<Published>()
                .WithTransaction(dbTransaction)
                .AsTable(_pubName)
                .AppendData(new Published
                {
                    Id = long.Parse(message.DbId),
                    Name = name,
                    Content = message.Content,
                    Retries = message.Retries,
                    Added = message.Added,
                    ExpiresAt = message.ExpiresAt,
                    StatusName = StatusName.Scheduled.ToString(),
                    Version = _capOptions.Value.Version
                })
                .ExecuteAffrowsAsync();
        }
        else
        {
            await _freeSql.Insert<Published>()
                .AsTable(_pubName)
                .AppendData(new Published
                {
                    Id = long.Parse(message.DbId),
                    Name = name,
                    Content = message.Content,
                    Retries = message.Retries,
                    Added = message.Added,
                    ExpiresAt = message.ExpiresAt,
                    StatusName = StatusName.Scheduled.ToString(),
                    Version = _capOptions.Value.Version
                })
                .ExecuteAffrowsAsync();
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
            Id = long.Parse(_snowflakeId.NextId().ToString()),
            Name = name,
            Group = group,
            Content = content,
            Retries = _capOptions.Value.FailedRetryCount,
            Added = DateTime.Now,
            ExpiresAt = DateTime.Now.AddSeconds(_capOptions.Value.FailedMessageExpiredAfter),
            StatusName = nameof(StatusName.Failed),
            Version = _capOptions.Value.Version
        };

        _freeSql.Insert<Received>()
            .AsTable(_recName)
            .AppendData(dto)
            .ExecuteAffrows();
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
            Version = _capOptions.Value.Version
        };
        await _freeSql.Insert<Received>()
            .AsTable(_recName)
            .AppendData(rec)
            .ExecuteAffrowsAsync();
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
        return await _freeSql.Select<Received>()
            .AsTable((_, _) => table)
            .Where(p =>
                p.ExpiresAt < timeout &&
                (p.StatusName == StatusName.Succeeded.ToString() || p.StatusName == StatusName.Failed.ToString())
            )
            .Limit(batchCount)
            .ToDelete()
            .ExecuteAffrowsAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<MediumMessage>> GetPublishedMessagesOfNeedRetry()
    {
        var fourMinAgo = DateTime.Now.AddMinutes(-4);
        var result = await _freeSql.Queryable<Published>()
            .AsTable((_, _) => _pubName)
            .Where(p => p.Retries < _capOptions.Value.FailedRetryCount)
            .Where(p => p.Version == _capOptions.Value.Version)
            .Where(p => p.Added < fourMinAgo)
            .Where(p => p.StatusName == StatusName.Failed.ToString() ||
                        p.StatusName == StatusName.Scheduled.ToString())
            .Limit(200)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scheduleTask"></param>
    /// <param name="token"></param>
    public async Task ScheduleMessagesOfDelayedAsync(Func<object, IEnumerable<MediumMessage>, Task> scheduleTask,
        CancellationToken token = new())
    {
        var result = await _freeSql.Select<Published>()
            .AsTable((_, _) => _pubName)
            .Where(p => p.Version == _capOptions.Value.Version)
            .Where(p =>
                (p.ExpiresAt < DateTime.Now.AddMinutes(2) && p.StatusName == StatusName.Delayed.ToString()) ||
                (p.ExpiresAt < DateTime.Now.AddMinutes(-1) && p.StatusName == StatusName.Queued.ToString()))
            .ToListAsync(token);

        if (result == null || result.Count == 0)
        {
            await scheduleTask(token, new List<MediumMessage>());
            return;
        }

        var messageList = result.Select(p => new MediumMessage
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
        var result = await _freeSql.Queryable<Received>()
            .AsTable((_, _) => _recName)
            .Where(p => p.Retries < _capOptions.Value.FailedRetryCount)
            .Where(p => p.Version == _capOptions.Value.Version)
            .Where(p => p.Added < fourMinAgo)
            .Where(p => p.StatusName == StatusName.Failed.ToString() ||
                        p.StatusName == StatusName.Scheduled.ToString())
            .Limit(200)
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IMonitoringApi GetMonitoringApi()
    {
        return new FreeSqlMonitoringApi(_initializer, _serializer, _freeSql);
    }

    private async Task ChangeMessageStateAsync(string tableName, MediumMessage message, StatusName state,
        object? transaction = null)
    {
        var dbId = long.Parse(message.DbId);
        if (transaction is DbTransaction dbTransaction)
        {
            await _freeSql.Update<Published>()
                .WithTransaction(dbTransaction)
                .AsTable(tableName)
                .Set(a => a.Content, _serializer.Serialize(message.Origin))
                .Set(a => a.Retries, message.Retries)
                .Set(a => a.ExpiresAt, message.ExpiresAt)
                .Set(a => a.StatusName, state.ToString())
                .Where(a => a.Id == dbId)
                .ExecuteAffrowsAsync()
                .ConfigureAwait(false);
        }
        else
        {
            await _freeSql.Update<Published>()
                .AsTable(tableName)
                .Set(a => a.Content, _serializer.Serialize(message.Origin))
                .Set(a => a.Retries, message.Retries)
                .Set(a => a.ExpiresAt, message.ExpiresAt)
                .Set(a => a.StatusName, state.ToString())
                .Where(a => a.Id == dbId)
                .ExecuteAffrowsAsync()
                .ConfigureAwait(false);
        }
    }
}