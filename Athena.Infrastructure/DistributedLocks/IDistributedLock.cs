namespace Athena.Infrastructure.DistributedLocks;

/// <summary>
/// 分布式锁接口
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// 在集群中锁定一个关键词。如果锁成功则返回；如果锁失败，则等待1分钟继续尝试锁，直到锁成功
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param> 
    Task<ILockResource> LockAsync(string resourceName, string key);

    /// <summary>
    /// 在集群中锁定一个关键词。如果锁成功则返回；如果锁失败，则等待一段时间继续尝试锁，直到锁成功
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param>
    /// <param name="timeSpan">有效时间</param> 
    Task<ILockResource> LockAsync(string resourceName, string key, TimeSpan timeSpan);

    /// <summary>
    /// 试图锁一个关键词，如果锁成功，则返回true，否则返回false，不会持续等待,锁的有效期是一分钟
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param> 
    /// <returns>如果锁成功，则返回true，否则返回false</returns>
    Task<ILockResource?> TryLockAsync(string resourceName, string key);

    /// <summary>
    /// 试图锁一个关键词，如果锁成功，则返回true，否则返回false，不会持续等待
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param>
    /// <param name="timeSpan">要锁定的时间</param> 
    /// <returns>如果锁成功，则返回锁资源，否则返回null</returns>
    Task<ILockResource?> TryLockAsync(string resourceName, string key, TimeSpan timeSpan);
}