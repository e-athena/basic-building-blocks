// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务组件扩展类
/// </summary>
public static class ComponentExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblyKeyword"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomServiceComponent(this IServiceCollection services,
        string? assemblyKeyword = null)
    {
        services.AddCustomServiceComponent(AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword));
        return services;
    }

    /// <summary>
    /// 添加服务组件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomServiceComponent(this IServiceCollection services,
        params string[] assemblyKeywords)
    {
        services.AddCustomServiceComponent(AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords));
        return services;
    }

    /// <summary>
    /// 添加服务组件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomServiceComponent(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes().Where(IsServiceComponent)))
        {
            if (type.GetCustomAttributes(typeof(ComponentAttribute), false)
                    .FirstOrDefault() is not ComponentAttribute t)
            {
                continue;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                switch (t.LifeStyle)
                {
                    case LifeStyle.Scoped:
                        services.AddScoped(interfaceType, type);
                        break;
                    case LifeStyle.Transient:
                        services.AddTransient(interfaceType, type);
                        break;
                    case LifeStyle.Singleton:
                        services.AddSingleton(interfaceType, type);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Check whether a type is a component type.
    /// </summary>
    private static bool IsServiceComponent(Type type)
    {
        return type is {IsClass: true, IsAbstract: false} &&
               type.GetCustomAttributes(typeof(ComponentAttribute), false).Any();
    }
}