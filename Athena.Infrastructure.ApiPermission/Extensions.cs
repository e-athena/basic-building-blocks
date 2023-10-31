// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加自定义权限
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomApiPermission(
        this IServiceCollection services,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        services.AddSingleton<IApiPermissionService, ApiPermissionService>();
        services.AddSingleton<IApiPermissionCacheService, ApiPermissionCacheService>();
        configureMoreActions?.Invoke(services);

        return services;
    }
}