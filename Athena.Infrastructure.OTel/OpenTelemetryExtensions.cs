using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
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
    /// <param name="meterProviderBuilderAction"></param>
    /// <param name="logProcessorBuilderAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomOpenTelemetry<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TracerProviderBuilder>? tracerProviderBuilderAction = null,
        Action<MeterProviderBuilder>? meterProviderBuilderAction = null,
        Action<LoggerProviderBuilder>? logProcessorBuilderAction = null
    )
    {
        // Note: Switch between Zipkin/OTLP/Console by setting UseTracingExporter in appsettings.json.
        var tracingExporter = configuration.GetValue("UseTracingExporter", defaultValue: "console")!
            .ToLowerInvariant();

        // Note: Switch between Prometheus/OTLP/Console by setting UseMetricsExporter in appsettings.json.
        var metricsExporter = configuration.GetValue("UseMetricsExporter", defaultValue: "console")!
            .ToLowerInvariant();

        // Note: Switch between Console/OTLP by setting UseLogExporter in appsettings.json.
        var logExporter = configuration.GetValue("UseLogExporter", defaultValue: "console")!.ToLowerInvariant();

        // Note: Switch between Explicit/Exponential by setting HistogramAggregation in appsettings.json
        var histogramAggregation = configuration.GetValue("HistogramAggregation", defaultValue: "explicit")!
            .ToLowerInvariant();

        services.AddOpenTelemetry()
            // Build a resource configuration action to set service information.
            .ConfigureResource(r => r.AddService(
                serviceName: configuration.GetEnvValue<string>("ServiceName") ?? "unknown",
                serviceVersion: typeof(T).Assembly.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName)
            )
            .WithTracing(builder =>
                {
                    builder
                        .SetSampler(new AlwaysOnSampler())
                        .AddCapInstrumentation()
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithException += (activity, exception) =>
                            {
                                activity.AddTag("exception", exception.Message);
                                activity.AddTag("exception.stacktrace", exception.StackTrace);
                                activity.AddTag("exception.type", exception.GetType().Name);
                                activity.AddTag("exception.source", exception.Source);
                                activity.AddTag("exception.hresult", exception.HResult.ToString());
                                activity.AddTag("exception.data", exception.Data.ToString());
                            };
                        })
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithException += (activity, exception) =>
                            {
                                activity.AddTag("exception", exception.Message);
                                activity.AddTag("exception.stacktrace", exception.StackTrace);
                                activity.AddTag("exception.type", exception.GetType().Name);
                                activity.AddTag("exception.source", exception.Source);
                                activity.AddTag("exception.hresult", exception.HResult.ToString());
                                activity.AddTag("exception.data", exception.Data.ToString());
                            };
                        });

                    switch (tracingExporter)
                    {
                        case "zipkin":
                            builder.AddZipkinExporter(o =>
                            {
                                var endpoint = configuration.GetEnvValue<string>("Zipkin:Endpoint");
                                endpoint ??= "http://localhost:9411/api/v2/spans";
                                o.Endpoint = new Uri(endpoint);
                            });
                            break;
                        case "otlp":
                            builder.AddOtlpExporter(otlpOptions =>
                            {
                                // Use IConfiguration directly for Otlp exporter endpoint option.
                                otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint",
                                    defaultValue: "http://localhost:4317")!);
                            });
                            break;

                        default:
                            builder.AddConsoleExporter();
                            break;
                    }

                    tracerProviderBuilderAction?.Invoke(builder);
                }
            )
            .WithMetrics(builder =>
            {
                // Metrics

                // Ensure the MeterProvider subscribes to any custom Meters.
                builder
                    // .AddMeter(Instrumentation.MeterName)
                    .SetExemplarFilter(ExemplarFilterType.TraceBased)
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                switch (histogramAggregation)
                {
                    case "exponential":
                        builder.AddView(instrument =>
                            instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                                ? new Base2ExponentialBucketHistogramConfiguration()
                                : null);
                        break;
                }

                switch (metricsExporter)
                {
                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint",
                                defaultValue: "http://localhost:4317")!);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }

                meterProviderBuilderAction?.Invoke(builder);
            })
            .WithLogging(builder =>
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                        serviceName: configuration.GetValue("ServiceName", defaultValue: "otel_test")!,
                        serviceVersion: typeof(T).Assembly.GetName().Version?.ToString() ?? "unknown",
                        serviceInstanceId: Environment.MachineName
                    )
                );
                switch (logExporter)
                {
                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            // Use IConfiguration directly for Otlp exporter endpoint option.
                            otlpOptions.Endpoint = new Uri(configuration.GetValue("Otlp:Endpoint",
                                defaultValue: "http://localhost:4317")!);
                        });
                        break;
                    default:
                        builder.AddConsoleExporter();
                        break;
                }

                logProcessorBuilderAction?.Invoke(builder);
            });
        return services;
    }
}