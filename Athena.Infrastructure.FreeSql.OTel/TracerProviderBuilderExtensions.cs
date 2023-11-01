// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

/// <summary>
/// 
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tracerProviderBuilder"></param>
    /// <returns></returns>
    public static TracerProviderBuilder AddFreeSqlInstrumentation(
        this TracerProviderBuilder tracerProviderBuilder)
    {
        return tracerProviderBuilder
            .AddSource(FreeSqlOTelActivityManager.ActivitySourceName)
            .AddInstrumentation(FreeSqlOTelActivityManager.Instance);
    }
}