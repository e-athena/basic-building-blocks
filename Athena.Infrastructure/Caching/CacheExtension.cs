namespace Athena.Infrastructure.Caching;

/// <summary>
/// [扩展类]缓存管理
/// </summary>
public static class CacheExtension
{
    /// <summary>
    /// 获取缓存值，如果缓存不存在，则读取数据源数据并添加至缓存
    ///     缓存60分钟
    /// </summary>
    /// <typeparam name="T">缓存类型</typeparam>
    /// <param name="cacheManager">缓存管理对象</param>
    /// <param name="key">键</param>
    /// <param name="acquire">获取数据源的方法</param>
    /// <returns></returns>
    public static T? Get<T>(this ICacheManager cacheManager, string key, Func<T?> acquire)
    {
        return Get(cacheManager, key, 60, acquire);
    }

    /// <summary>
    /// 获取缓存值，如果缓存不存在，则读取数据源数据并添加至缓存
    /// </summary>
    /// <typeparam name="T">缓存类型</typeparam>
    /// <param name="cacheManager">缓存管理对象</param>
    /// <param name="key">键</param>
    /// <param name="cacheTime">缓存时间/分钟</param>
    /// <param name="acquire">获取数据源的方法</param>
    /// <returns></returns>
    public static T? Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
    {
        return cacheTime > 0
            ? cacheManager.GetOrCreate(key, acquire, TimeSpan.FromMinutes(cacheTime))
            : cacheManager.GetOrCreate(key, acquire);
    }

    /// <summary>
    /// 获取缓存值，如果缓存不存在，则读取数据源数据并添加至缓存
    /// </summary>
    /// <typeparam name="T">缓存类型</typeparam>
    /// <param name="cacheManager">缓存管理对象</param>
    /// <param name="key">键</param>
    /// <param name="cacheTime">缓存时间/分钟</param>
    /// <param name="acquire">获取数据源的方法</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T?> GetAsync<T>(this ICacheManager cacheManager, string key, int cacheTime,
        Func<Task<T>> acquire, CancellationToken cancellationToken = default)
    {
        if (cacheTime > 0)
        {
            return await cacheManager.GetOrCreateAsync(
                key,
                async () => await acquire(),
                TimeSpan.FromMinutes(cacheTime), cancellationToken);
        }

        return await cacheManager.GetOrCreateAsync(key, async () => await acquire(), cancellationToken);
    }

    /// <summary>
    /// 读取Redis配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    public static RedisConfig GetRedisConfig(
        this IConfiguration configuration,
        string configVariable = "RedisConfig",
        string envVariable = "REDIS_CONFIG")
    {
        return configuration.GetConfig<RedisConfig>(configVariable, envVariable);
    }
}