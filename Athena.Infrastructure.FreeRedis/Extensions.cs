// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展方法
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加Redis缓存
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <remarks>https://github.com/2881099/FreeRedis/blob/master/README.zh-CN.md</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeRedisCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
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

        var cli = new RedisClient(config.Configuration);
        cli.Serialize = obj => JsonSerializer.Serialize(obj);
        cli.Deserialize = (json, type) => JsonSerializer.Deserialize(json, type);
        RedisHelper.Initialization(cli);
        services.AddSingleton<ICacheManager>(new RedisCacheAdapter(config));

        return services;
    }

    /// <summary>
    /// 添加Redis缓存
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <remarks>https://github.com/2881099/FreeRedis/blob/master/README.zh-CN.md</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomFreeRedisCacheWithSentinels(
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

        var csRedis = new RedisClient(config.Configuration, config.Sentinels.ToArray(), true);
        csRedis.Serialize = obj => JsonSerializer.Serialize(obj);
        csRedis.Deserialize = (json, type) => JsonSerializer.Deserialize(json, type);
        RedisHelper.Initialization(csRedis);
        services.AddSingleton<ICacheManager>(new RedisCacheAdapter(config));
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