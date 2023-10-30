// ReSharper disable once CheckNamespace
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
}