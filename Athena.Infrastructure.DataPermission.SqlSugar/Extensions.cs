using Athena.Infrastructure.DataPermission.SqlSugar;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加数据权限
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomDataPermission(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.GetConfig<DataPermissionConfig>("DataPermissionConfig", "DATA_PERMISSION_CONFIG");
        // 如果CacheKeyFormat为空则设置默认值
        if (string.IsNullOrWhiteSpace(config.CacheKeyFormat))
        {
            config.CacheKeyFormat = "user:{0}:UserPolicyQuery:{1}";
        }

        // 如果CacheKeyFormat不包含{0}和{1}则提示错误
        if (!config.CacheKeyFormat.Contains("{0}") || !config.CacheKeyFormat.Contains("{1}"))
        {
            throw new AggregateException("CacheKeyFormat 必须包含{0}和{1}，示例：user:{0}:UserPolicyQuery:{1}");
        }

        // 启用缓存时缓存过期时间必须大于0
        if (config is {IsEnableCache: true, CacheExpireSeconds: <= 0})
        {
            throw new AggregateException("启用缓存时缓存过期时间必须大于0");
        }

        services.Configure<DataPermissionConfig>(cfg =>
        {
            cfg.IsEnableCache = config.IsEnableCache;
            cfg.CacheKeyFormat = config.CacheKeyFormat;
            cfg.CacheExpireSeconds = config.CacheExpireSeconds;
        });
        services.AddSingleton<IQueryFilterService, SqlSugarQueryFilterService>();
        return services;
    }
}