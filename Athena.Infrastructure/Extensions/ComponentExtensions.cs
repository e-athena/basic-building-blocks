// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务组件扩展类
/// </summary>
public static class ComponentExtensions
{
    /// <summary>
    /// 添加服务组件
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <remarks>可通过：Module:ComponentAssembly:Keyword、Module:ComponentAssembly:Keywords配置</remarks>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    public static IServiceCollection AddCustomServiceComponent(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var keywords = new List<string>();
        // 读取配置ComponentAssembly:Keyword
        var assemblyKeyword = configuration.GetValue<string>("Module:ComponentAssembly:Keyword");
        // 如果不为空，添加到关键字列表
        if (!string.IsNullOrEmpty(assemblyKeyword))
        {
            keywords.Add(assemblyKeyword);
        }

        // 读取配置ComponentAssembly:Keywords
        var assemblyKeywords = configuration.GetSection("Module:ComponentAssembly:Keywords").Get<string[]>();
        // 如果不为空，添加到关键字列表
        if (assemblyKeywords is not null && assemblyKeywords.Length > 0)
        {
            keywords.AddRange(assemblyKeywords);
        }

        // 添加服务组件
        services.AddCustomServiceComponent(keywords.ToArray());
        return services;
    }

    /// <summary>
    /// 添加服务组件
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