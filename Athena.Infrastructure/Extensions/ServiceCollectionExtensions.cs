// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 从程序集中注册服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        // 获取程序集中所有的公开类型
        var types = assembly.GetExportedTypes();

        foreach (var type in types)
        {
            // 获取类型实现的所有接口
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                // 将类型注册为其实现的每个接口的实现
                services.Add(new ServiceDescriptor(@interface, type, lifetime));
            }
        }

        return services;
    }

    /// <summary>
    /// 从程序集中注册服务，指定实现接口
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="interfaceType"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddServicesFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        Type interfaceType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        // 获取程序集中所有的公开类型
        var types = assembly.GetExportedTypes();

        foreach (var type in types)
        {
            // 获取类型实现的所有接口
            var interfaces = type.GetInterfaces();

            foreach (var @interface in interfaces)
            {
                if (@interface == interfaceType)
                {
                    // 将类型注册为其实现的每个接口的实现
                    services.Add(new ServiceDescriptor(@interface, type, lifetime));
                }
            }
        }

        return services;
    }
}