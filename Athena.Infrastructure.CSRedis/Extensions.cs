// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    /// <summary>
    /// 添加Redis缓存
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomCsRedisCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Eric的配置
        // 127.0.0.1:6379,poolsize=50,writeBuffer=102400,preheat=10,testcluster=false,autoDispose=false,syncTimeout=20000
        var config = configuration.GetRedisConfig();
        // 127.0.0.1[:6379],password=123456,defaultDatabase=13,poolsize=50,ssl=false,writeBuffer=10240,prefix=key前辍
        if (!config.Configuration.Contains("defaultDatabase"))
        {
            config.Configuration += $",defaultDatabase={config.DefaultDatabase}";
        }

        if (!config.Configuration.Contains("prefix"))
        {
            config.Configuration += $",prefix={config.InstanceName}";
        }

        var csRedis = new CSRedisClient(config.Configuration);
        RedisHelper.Initialization(csRedis);
        services.AddSingleton<ICacheManager, RedisCacheAdapter>();
        services.AddSingleton<IDistributedLock, DistributedLock>();
        services.AddSingleton<IDistributedCache>(new CSRedisCache(RedisHelper.Instance));

        return services;
    }

    /// <summary>
    /// 添加Redis缓存
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomCsRedisCacheWithSentinels(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var config = configuration.GetRedisConfig();
        if (config.Sentinels == null || config.Sentinels.Count == 0)
        {
            throw new ArgumentNullException(nameof(config.Sentinels), "[Sentinels]未配置");
        }

        // 127.0.0.1[:6379],password=123456,defaultDatabase=13,poolsize=50,ssl=false,writeBuffer=10240,prefix=key前辍
        if (!config.Configuration.Contains("defaultDatabase"))
        {
            config.Configuration += $",defaultDatabase={config.DefaultDatabase}";
        }

        if (!config.Configuration.Contains("prefix"))
        {
            config.Configuration += $",prefix={config.InstanceName}";
        }

        var csRedis = new CSRedisClient(config.Configuration, config.Sentinels.ToArray());
        RedisHelper.Initialization(csRedis);
        services.AddSingleton<ICacheManager, RedisCacheAdapter>();
        services.AddSingleton<IDistributedLock, DistributedLock>();
        services.AddSingleton<IDistributedCache>(new CSRedisCache(RedisHelper.Instance));

        return services;
    }


    /// <summary>
    /// 读取Redis配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static RedisConfig GetRedisConfig(
        this IConfiguration configuration,
        string configVariable = "RedisConfig",
        string envVariable = "REDIS_CONFIG")
    {
        return configuration.GetConfig<RedisConfig>(configVariable, envVariable);
    }
}