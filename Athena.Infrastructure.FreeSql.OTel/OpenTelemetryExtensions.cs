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
    public static IServiceCollection AddCustomOpenTelemetry<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TracerProviderBuilder>? tracerProviderBuilderAction = null)
    {
        services.AddOpenTelemetry()
            // Build a resource configuration action to set service information.
            .ConfigureResource(r => r.AddService(
                serviceName: configuration.GetValue<string>("ServiceName") ?? "unknown",
                serviceVersion: typeof(T).Assembly.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName)
            )
            .WithTracing(providerBuilder =>
                {
                    providerBuilder
                        .SetSampler(new AlwaysOnSampler())
                        .AddFreeSqlInstrumentation()
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
                        })
                        .AddZipkinExporter(o =>
                        {
                            var endpoint = configuration.GetValue<string>("Zipkin:Endpoint");
                            endpoint ??= "http://localhost:9411/api/v2/spans";
                            o.Endpoint = new Uri(endpoint);
                        });
                    tracerProviderBuilderAction?.Invoke(providerBuilder);
                }
            );
        return services;
    }
}