// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Athena.Infrastructure.FreeSql.CAPs.Extends.Models;
using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.Options;

namespace Athena.Infrastructure.FreeSql.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class FreeSqlStorageInitializer : IStorageInitializer
{
    private readonly ILogger _logger;
    private readonly IFreeSql _freeSql;
    private readonly IOptions<CapOptions> _capOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="freeSql"></param>
    /// <param name="capOptions"></param>
    public FreeSqlStorageInitializer(
        ILoggerFactory loggerFactory, IFreeSql freeSql, IOptions<CapOptions> capOptions)
    {
        _freeSql = freeSql;
        _capOptions = capOptions;
        _logger = loggerFactory.CreateLogger<FreeSqlStorageInitializer>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetPublishedTableName()
    {
        return CapConstant.PublishedTableName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetReceivedTableName()
    {
        return CapConstant.ReceivedTableName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetLockTableName()
    {
        return CapConstant.LockTableName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        _freeSql.CodeFirst.SyncStructure<Published>();
        _freeSql.CodeFirst.SyncStructure<Received>();
        _freeSql.CodeFirst.SyncStructure<Lock>();

        // 添加锁
        var key1 = $"publish_retry_{_capOptions.Value.Version}";
        // 如果不存在则添加
        var any1 = _freeSql.Queryable<Lock>()
            .Where(p => p.Key == key1)
            .Any();
        if (!any1)
        {
            _freeSql.Insert(new Lock
            {
                Key = $"publish_retry_{_capOptions.Value.Version}",
                Instance = "",
                LastLockTime = DateTime.MinValue
            }).ExecuteAffrows();
        }

        // 添加锁
        var key2 = $"received_retry_{_capOptions.Value.Version}";
        // 如果不存在则添加
        var any2 = _freeSql.Queryable<Lock>()
            .Where(p => p.Key == key1)
            .Any();
        if (!any1)
        {
            _freeSql.Insert(new Lock
            {
                Key = $"received_retry_{_capOptions.Value.Version}",
                Instance = "",
                LastLockTime = DateTime.MinValue
            }).ExecuteAffrows();
        }

        _logger.LogDebug("Ensuring all create database tables script are applied");
        return Task.CompletedTask;
    }
}