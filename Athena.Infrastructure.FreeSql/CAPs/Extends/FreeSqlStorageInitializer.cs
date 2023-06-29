// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Athena.Infrastructure.FreeSql.CAPs.Extends.Models;
using DotNetCore.CAP.Persistence;

namespace Athena.Infrastructure.FreeSql.CAPs.Extends;

/// <summary>
/// 
/// </summary>
public class FreeSqlStorageInitializer : IStorageInitializer
{
    private readonly ILogger _logger;
    private readonly IFreeSql _freeSql;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="freeSql"></param>
    public FreeSqlStorageInitializer(
        ILoggerFactory loggerFactory, IFreeSql freeSql)
    {
        _freeSql = freeSql;
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
        _logger.LogDebug("Ensuring all create database tables script are applied");
        return Task.CompletedTask;
    }
}