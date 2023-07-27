namespace Athena.Infrastructure.CSRedis;

/// <summary>
/// 分布式锁接口实现
/// </summary>
public sealed class DistributedLock : IDistributedLock
{
    private readonly TimeSpan _defaultExpiredTimeSpan = new(0, 1, 0);

    /// <summary>
    /// 在集群中锁定一个关键词。如果锁成功则返回；如果锁失败，则等待1分钟继续尝试锁，直到锁成功
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param> 
    public async Task<ILockResource> LockAsync(string resourceName, string key)
    {
        var lockResource = new LockResource(resourceName, key);
        while (true)
        {
            var res = await lockResource.LockAsync(_defaultExpiredTimeSpan);
            if (res != null)
            {
                return res;
            }

            await Task.Delay(20);
        }
    }

    /// <summary>
    /// 在集群中锁定一个关键词。如果锁成功则返回；如果锁失败，则等待一段时间继续尝试锁，直到锁成功
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param>
    /// <param name="timeSpan">有效时间</param> 
    public async Task<ILockResource> LockAsync(string resourceName, string key, TimeSpan timeSpan)
    {
        var lockResource = new LockResource(resourceName, key);
        while (true)
        {
            var res = await lockResource.LockAsync(timeSpan);
            if (res != null)
            {
                return res;
            }

            await Task.Delay(20);
        }
    }

    /// <summary>
    /// 试图锁一个关键词，如果锁成功，则返回true，否则返回false，不会持续等待,锁的有效期是一分钟
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param> 
    /// <returns>如果锁成功，则返回true，否则返回false</returns>
    public async Task<bool> TryLockAsync(string resourceName, string key)
    {
        var lockResult = await TryGetLockAsync(resourceName, key);
        return lockResult != null;
    }

    /// <summary>
    /// 试图锁一个关键词，如果锁成功，则返回true，否则返回false，不会持续等待
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param>
    /// <param name="timeSpan">要锁定的时间</param> 
    /// <returns>如果锁成功，则返回锁资源，否则返回null</returns>
    public async Task<bool> TryLockAsync(string resourceName, string key, TimeSpan timeSpan)
    {
        var lockResult = await TryGetLockAsync(resourceName, key, timeSpan);
        return lockResult != null;
    }

    public Task<ILockResource?> TryGetLockAsync(string resourceName, string key)
    {
        var lockResource = new LockResource(resourceName, key);
        return lockResource.LockAsync(_defaultExpiredTimeSpan);
    }

    public Task<ILockResource?> TryGetLockAsync(string resourceName, string key, TimeSpan timeSpan)
    {
        var lockResource = new LockResource(resourceName, key);
        return lockResource.LockAsync(timeSpan);
    }
}

/// <summary>
/// 锁资源
/// </summary>
public class LockResource : ILockResource
{
    private const string Prefix = "DistributedLock_";
    private const int LockKeyLengthLimit = 256;

    private const string UnlockCommand = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";

    //SET lockName lockValue NX PX 30000 ms
    private const string LockCommand = @"
            if redis.call(""set"",KEYS[1],ARGV[1],ARGV[2],ARGV[3],ARGV[4]) then
                return 0
            else 
                return 1
            end";

    /// <summary>
    /// 请求锁的请求唯一识别码,同一个请求的锁只能被同个请求解锁
    /// </summary>
    private readonly string _requestId;

    private readonly string _resourceName;
    private readonly string _key;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    public LockResource(string resourceName, string key)
    {
        _requestId = Guid.NewGuid().ToString();
        _resourceName = resourceName;
        _key = key;
    }

    /// <summary>
    /// 通过redis 加锁
    /// </summary>  
    /// <param name="timeSpan">timeSpan</param> 
    /// <returns>布尔结果</returns>
    public async Task<ILockResource?> LockAsync(TimeSpan timeSpan)
    {
        var k = GetPersistentKey(_resourceName, _key);
        var result = await RedisHelper
            .EvalAsync(LockCommand, k,
                _requestId,
                "NX",
                "PX",
                timeSpan.TotalMilliseconds + string.Empty
            )
            .ConfigureAwait(false);
        var res = result?.ToString() == "0" ? this : null;
        if (EnvironmentHelper.IsDevelopment)
        {
            Console.WriteLine(res == null ? "获取锁失败，{0}" : "获取锁成功，{0}", k);
        }

        return res;
    }

    /// <summary>
    /// 释放锁资源
    /// </summary> 
    public async Task ReleaseAsync()
    {
        var k = GetPersistentKey(_resourceName, _key);
        await RedisHelper.EvalAsync(UnlockCommand, k, _requestId).ConfigureAwait(false);
        if (EnvironmentHelper.IsDevelopment)
        {
            Console.WriteLine("释放锁成功，{0}", k);
        }
    }

    private static string GetPersistentKey(string resourceName, string key)
    {
        var newKey = Prefix + resourceName + "." + key;
        if (newKey.Length > LockKeyLengthLimit)
        {
            throw new Exception($"The key length of {key} is too long.");
        }

        return newKey;
    }
}