using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="tracerProviderBuilderAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomOpenTelemetryFreeSql<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TracerProviderBuilder>? tracerProviderBuilderAction = null)
    {
        services.AddCustomOpenTelemetry<T>(
            configuration,
            actions =>
            {
                //
                tracerProviderBuilderAction?.Invoke(actions.AddFreeSqlInstrumentation());
            });
        return services;
    }
}