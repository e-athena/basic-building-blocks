using DotNetCore.CAP.Transport;

namespace Athena.Infrastructure.FreeSql.CAPs;

/// <summary>
/// CAP事件扩展
/// </summary>
public static class CapTransactionExtensions
{
    /// <summary>
    /// 开启事务
    /// </summary>
    /// <param name="unitOfWork"></param>
    /// <param name="publisher"></param>
    /// <param name="autoCommit"></param>
    /// <returns></returns>
    public static ICapTransaction BeginTransaction(this IUnitOfWork unitOfWork, ICapPublisher publisher,
        bool autoCommit = false)
    {
        var dispatcher = publisher.ServiceProvider.GetRequiredService<IDispatcher>();
        var transaction = new FreeSqlRepositoryPatternTransaction(dispatcher, unitOfWork)
        {
            AutoCommit = autoCommit
        };
        return publisher.Transaction.Value = transaction;
    }
}