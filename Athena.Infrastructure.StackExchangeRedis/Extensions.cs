namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    /// <summary>
    /// 添加Redis缓存
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomStackExchangeRedisCache(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<RedisCacheOptions>? setupAction = null
    )
    {
        services.AddSingleton<ICacheManager, RedisCacheAdapter>();
        services.AddStackExchangeRedisCache(options =>
        {
            var config = configuration.GetRedisConfig();
            var configurationOptions = ConfigurationOptions.Parse(config.Configuration);
            configurationOptions.DefaultDatabase = config.DefaultDatabase;
            options.ConfigurationOptions = configurationOptions;
            options.InstanceName = config.InstanceName;

            setupAction?.Invoke(options);
        });

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