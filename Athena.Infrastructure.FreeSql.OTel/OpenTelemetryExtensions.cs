using Microsoft.Extensions.Configuration;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
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
    /// <param name="meterProviderBuilderAction"></param>
    /// <param name="logProcessorBuilderAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomOpenTelemetryFreeSql<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TracerProviderBuilder>? tracerProviderBuilderAction = null,
        Action<MeterProviderBuilder>? meterProviderBuilderAction = null,
        Action<LoggerProviderBuilder>? logProcessorBuilderAction = null
    )
    {
        services.AddSingleton<FreeSqlInstrumentation>();
        services.AddCustomOpenTelemetry<T>(configuration, tracerBuilder =>
            {
                tracerBuilder.AddFreeSqlInstrumentation();
                tracerProviderBuilderAction?.Invoke(tracerBuilder);
            },
            meterBuilder =>
            {
                meterBuilder.AddFreeSqlInstrumentation();
                meterProviderBuilderAction?.Invoke(meterBuilder);
            },
            logProcessorBuilderAction
        );
        return services;
    }
}