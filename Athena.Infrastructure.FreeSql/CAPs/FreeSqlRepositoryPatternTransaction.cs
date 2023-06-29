using DotNetCore.CAP.Transport;

namespace Athena.Infrastructure.FreeSql.CAPs;

/// <summary>
/// 
/// </summary>
public class FreeSqlRepositoryPatternTransaction : CapTransactionBase
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="uow"></param>
    public FreeSqlRepositoryPatternTransaction(IDispatcher dispatcher, IUnitOfWork uow) : base(dispatcher)
    {
        _unitOfWork = uow;
    }

    /// <summary>
    /// 事务
    /// </summary>
    public override object? DbTransaction => _unitOfWork.GetOrBeginTransaction();

    /// <summary>
    /// 提交
    /// </summary>
    public override void Commit()
    {
        _unitOfWork.Commit();
        Flush();
    }

    /// <summary>
    /// 提交
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commit();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 回滚
    /// </summary>
    public override void Rollback()
    {
        _unitOfWork.Rollback();
    }

    /// <summary>
    /// 回滚
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Rollback();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public override void Dispose()
    {
        _unitOfWork.Dispose();
    }
}