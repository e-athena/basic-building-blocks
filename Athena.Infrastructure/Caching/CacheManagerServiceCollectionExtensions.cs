using Athena.Infrastructure.Caching;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public static class CacheManagerServiceCollectionExtensions
{

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomDistributedCaching(this ServiceCollection services, Assembly assembly)
    {
        services.AddServicesFromAssembly(assembly, typeof(ICacheManager), ServiceLifetime.Singleton);
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomDistributedCaching(
        this ServiceCollection services,
        IConfiguration configuration
    )
    {
        var keywords = AssemblyHelper.GetAssemblyKeywords(configuration, "");
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(keywords);
        foreach (var assembly in assemblies)
        {
            services.AddServicesFromAssembly(assembly, typeof(ICacheManager), ServiceLifetime.Singleton);
        }

        return services;
    }
}