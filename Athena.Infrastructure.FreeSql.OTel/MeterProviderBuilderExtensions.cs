using OpenTelemetry.Metrics;


// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

/// <summary>
/// 
/// </summary>
public static class MeterProviderBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MeterProviderBuilder AddFreeSqlInstrumentation(
        this MeterProviderBuilder builder)
    {
        return builder
            .AddMeter(FreeSqlOTelActivityManager.ActivitySourceName)
            .AddInstrumentation(FreeSqlOTelActivityManager.Instance);
    }
}