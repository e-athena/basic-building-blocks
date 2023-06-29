namespace Athena.Infrastructure.DistributedLocks;

/// <summary>
/// 锁资源接口
/// </summary>
public interface ILockResource
{
    /// <summary>
    /// 锁定资源
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    Task<ILockResource?> LockAsync(TimeSpan timeSpan);

    /// <summary>
    /// 释放锁定的资源
    /// </summary>
    /// <returns></returns>
    Task ReleaseAsync();
}