

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class AthenaProviderServiceExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAthenaProvider(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAthenaProvider, AthenaProvider>();
        return services;
    }
}