// Copyright (c) .NET Core Community. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DotNetCore.CAP.Transport;

// ReSharper disable once CheckNamespace
namespace DotNetCore.CAP;

/// <summary>
/// 
/// </summary>
public class SqlSugarCapTransaction : CapTransactionBase
{
    private readonly IDbTransaction _dbTransaction;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="dbConnection"></param>
    /// <param name="isolationLevel"></param>
    public SqlSugarCapTransaction(IDispatcher dispatcher, IDbConnection dbConnection,
        IsolationLevel? isolationLevel) : base(dispatcher)
    {
        _dbTransaction = isolationLevel.HasValue
            ? dbConnection.BeginTransaction(isolationLevel.Value)
            : dbConnection.BeginTransaction();
    }

    /// <summary>
    /// 事务
    /// </summary>
    public override object DbTransaction => _dbTransaction;

    /// <summary>
    /// 
    /// </summary>
    public override void Commit()
    {
        Debug.Assert(DbTransaction != null);
        _dbTransaction.Commit();
        Flush();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commit();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Rollback()
    {
        Debug.Assert(DbTransaction != null);
        _dbTransaction.Rollback();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Rollback();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Dispose()
    {
        _dbTransaction.Dispose();
    }
}

/// <summary>
/// 
/// </summary>
public static class CapTransactionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="autoCommit"></param>
    /// <returns></returns>
    public static ICapTransaction Begin(this ICapTransaction transaction, bool autoCommit = false)
    {
        transaction.AutoCommit = autoCommit;
        return transaction;
    }

    /// <summary>
    /// Start the CAP transaction
    /// </summary>
    /// <param name="publisher">The <see cref="ICapPublisher" />.</param>
    /// <param name="dbConnection"></param>
    /// <param name="isolationLevel"></param>
    /// <param name="autoCommit">Whether the transaction is automatically committed when the message is published</param>
    /// <returns>The <see cref="ICapTransaction" /> object.</returns>
    public static ICapTransaction BeginTransaction(this ICapPublisher publisher, IDbConnection dbConnection,
        IsolationLevel? isolationLevel = null,
        bool autoCommit = false)
    {
        var dispatcher = publisher.ServiceProvider.GetRequiredService<IDispatcher>();
        publisher.Transaction.Value = new SqlSugarCapTransaction(dispatcher, dbConnection, isolationLevel);
        return publisher.Transaction.Value.Begin(autoCommit);
    }
}