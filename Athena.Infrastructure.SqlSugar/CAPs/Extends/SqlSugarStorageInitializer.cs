// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;
using DotNetCore.CAP.Persistence;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class SqlSugarStorageInitializer : IStorageInitializer
{
    private readonly ILogger _logger;
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="sqlSugarClient"></param>
    public SqlSugarStorageInitializer(
        ILoggerFactory loggerFactory, ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        _sqlSugarClient.CodeFirst.InitTables(new[]
        {
            typeof(Published),
            typeof(Received)
        });
        _logger.LogDebug("Ensuring all create database tables script are applied");
        return Task.CompletedTask;
    }
}