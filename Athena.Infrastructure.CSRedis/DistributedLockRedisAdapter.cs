namespace Athena.Infrastructure.CSRedis;

/// <summary>
/// [CsRedis原生]分布式锁接口实现
/// </summary>
public sealed class DistributedLockRedisAdapter : IDistributedLock
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly TimeSpan _defaultExpiredTimeSpan = new(0, 1, 0);

    /// <summary>
    ///
    /// </summary>
    /// <param name="loggerFactory"></param>
    public DistributedLockRedisAdapter(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    private const double BaseDelay = 100;
    private const double MaxDelay = 10000;

    /// <summary>
    /// 在集群中锁定一个关键词。如果锁成功则返回；如果锁失败，则等待1分钟继续尝试锁，直到锁成功
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">要锁定的关键词</param> 
    public async Task<ILockResource> LockAsync(string resourceName, string key)
    {
        var lockResource = new LockResourceRedisAdapter(resourceName, key);
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
        var lockResource = new LockResourceRedisAdapter(resourceName, key);
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
        var lockResource = new LockResourceRedisAdapter(resourceName, key);
        return lockResource.LockAsync(_defaultExpiredTimeSpan);
    }

    public Task<ILockResource?> TryGetLockAsync(string resourceName, string key, TimeSpan timeSpan)
    {
        var lockResource = new LockResourceRedisAdapter(resourceName, key);
        return lockResource.LockAsync(timeSpan);
    }

    /// <summary>
    /// 尝试异步获取分布式锁
    /// </summary>
    /// <param name="resourceName">服务的名称</param>
    /// <param name="key">锁的唯一标识符</param>
    /// <param name="timeout">尝试获取锁的超时时间</param>
    /// <param name="expiry">锁的有效期，如果未指定，默认为无限期</param>
    /// <returns>返回一个元组，包含锁对象和一个布尔值表示是否成功获取锁</returns>
    public async Task<ILockResource?> TryAcquireLockAsync(string resourceName, string key, TimeSpan timeout,
        TimeSpan? expiry = null)
    {
        var lockResource = new LockResourceRedisAdapter(resourceName, key);
        if (timeout == TimeSpan.MaxValue)
        {
            timeout = Timeout.InfiniteTimeSpan;
        }

        using var cts = new CancellationTokenSource(timeout);
        var retries = 0.0;

        while (!cts.IsCancellationRequested)
        {
            var locker = await lockResource.LockAsync(expiry ?? TimeSpan.MaxValue);
            if (locker != null)
            {
                return locker;
            }

            try
            {
                await Task.Delay(GetDelay(++retries), cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
        }

        return null;
    }

    /// <summary>
    /// 100     100
    /// 164     171
    /// 256     312
    /// 401     519
    /// 754     766
    /// 1327    1562
    /// 2950    3257
    /// 4596    4966
    /// 7215    8667
    /// 10000   10000
    /// </summary>
    /// <param name="retries"></param>
    /// <returns></returns>
    private static TimeSpan GetDelay(double retries)
    {
        var delay = BaseDelay *
                    (1.0 + ((Math.Pow(1.8, retries - 1.0) - 1.0) * (0.6 + new Random().NextDouble() * 0.4)));
        return TimeSpan.FromMilliseconds(Math.Min(delay, MaxDelay));
    }
}

/// <summary>
/// 锁资源
/// </summary>
public class LockResourceRedisAdapter : ILockResource
{
    private const string Prefix = "DistributedLock_";
    private const int LockKeyLengthLimit = 256;

    private readonly string _resourceName;
    private readonly string _key;
    private CSRedisClientLock? _csRedisClientLock;
    private bool _disposed;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="key"></param>
    public LockResourceRedisAdapter(string resourceName, string key)
    {
        _resourceName = resourceName;
        _key = key;
    }

    /// <summary>
    /// 通过redis 加锁
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    private ILockResource? Lock(TimeSpan timeSpan)
    {
        var key = GetPersistentKey(_resourceName, _key);
        _csRedisClientLock = RedisHelper.Lock(key, (int)timeSpan.TotalSeconds);
        return _csRedisClientLock != null ? this : null;
    }

    /// <summary>
    /// 通过redis 加锁
    /// </summary>  
    /// <param name="timeSpan">timeSpan</param> 
    /// <returns>布尔结果</returns>
    public Task<ILockResource?> LockAsync(TimeSpan timeSpan)
    {
        return Task.FromResult(Lock(timeSpan));
    }

    /// <summary>
    /// 释放锁资源
    /// </summary> 
    public Task ReleaseAsync()
    {
        _csRedisClientLock?.Unlock();
        return Task.CompletedTask;
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _csRedisClientLock?.Dispose();
        // 释放锁资源
        ReleaseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (_csRedisClientLock is IAsyncDisposable csRedisClientLockAsyncDisposable)
        {
            await csRedisClientLockAsyncDisposable.DisposeAsync();
        }
        else
        {
            _csRedisClientLock?.Dispose();
        }

        // 释放锁资源
        await ReleaseAsync();
    }
}