using Athena.Infrastructure;
using Athena.Infrastructure.Caching;
using Athena.Infrastructure.NewLifeRedis;
using Microsoft.Extensions.Configuration;
using NewLife.Caching;

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
    /// <remarks>https://newlifex.com/core/redis</remarks>
    /// <returns></returns>
    public static IServiceCollection AddCustomNewLifeRedisCache(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var config = configuration.GetRedisConfig();
        if (!config.Configuration.Contains("defaultDatabase"))
        {
            config.Configuration += $",defaultDatabase={config.DefaultDatabase}";
        }

        if (!config.Configuration.Contains("prefix"))
        {
            config.Configuration += $",prefix={config.InstanceName}";
        }

        var redis = new FullRedis();
        redis.Init(config.Configuration);
        RedisHelper.Initialization(redis);
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