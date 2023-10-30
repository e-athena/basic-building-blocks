// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 
/// </summary>
public static class SerilogHostBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureLogger"></param>
    /// <param name="preserveStaticLogger"></param>
    /// <param name="writeToProviders"></param>
    /// <returns></returns>
    public static WebApplicationBuilder UseCustomSerilog(
        this WebApplicationBuilder builder,
        Action<HostBuilderContext, LoggerConfiguration>? configureLogger = null,
        bool preserveStaticLogger = false,
        bool writeToProviders = false)
    {
        builder.Logging.ClearProviders();
        builder
            .Host
            .UseSerilog((ctx, cfg) =>
            {
                if (configureLogger == null)
                {
                    cfg.ReadFrom.Configuration(ctx.Configuration);
                }
                else
                {
                    configureLogger.Invoke(ctx, cfg);
                }
            }, preserveStaticLogger, writeToProviders);
        return builder;
    }
}