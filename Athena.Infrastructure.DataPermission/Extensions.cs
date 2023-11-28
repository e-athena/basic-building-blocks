

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 添加数据权限静态服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomDataPermissionStatic(
        this IServiceCollection services)
    {
        services.AddSingleton<IDataPermissionStaticService, DataPermissionStaticService>();
        return services;
    }
}