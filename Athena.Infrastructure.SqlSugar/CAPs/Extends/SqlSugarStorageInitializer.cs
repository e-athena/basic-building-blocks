// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using DotNetCore.CAP.Persistence;
using Microsoft.Extensions.Options;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class SqlSugarStorageInitializer : IStorageInitializer
{
    private readonly ILogger _logger;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly IOptions<CapOptions> _capOptions;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="sqlSugarClient"></param>
    /// <param name="capOptions"></param>
    public SqlSugarStorageInitializer(
        ILoggerFactory loggerFactory, ISqlSugarClient sqlSugarClient, IOptions<CapOptions> capOptions)
    {
        _sqlSugarClient = sqlSugarClient;
        _capOptions = capOptions;
        _logger = loggerFactory.CreateLogger<SqlSugarStorageInitializer>();
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
    /// <exception cref="NotImplementedException"></exception>
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
        
        SqlSugarClientHelper.AutoSyncCapMessageTable(_sqlSugarClient, _capOptions.Value.Version);
        _logger.LogDebug("Ensuring all create database tables script are applied");
        return Task.CompletedTask;
    }
}