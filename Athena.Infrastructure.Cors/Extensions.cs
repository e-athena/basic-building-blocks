// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    /// <summary>
    /// CORS
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="policyName"></param>
    /// <param name="setupOptions"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomCors(this IServiceCollection services,
        IConfiguration config,
        string policyName,
        Action<CorsOptions>? setupOptions = null,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, builder =>
            {
                var origins = config.GetCorsOrigins();
                if (origins == "*")
                {
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(_ => true);
                    return;
                }

                builder.WithOrigins(origins.Split(","))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
            setupOptions?.Invoke(options);
        });
        configureMoreActions?.Invoke(services);
        return services;
    }

    /// <summary>
    /// CORS
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="setupOptions"></param>
    /// <param name="configureMoreActions"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomCors(this IServiceCollection services,
        IConfiguration config,
        Action<CorsOptions>? setupOptions = null,
        Action<IServiceCollection>? configureMoreActions = null)
    {
        const string defaultPolicyName = "__DefaultCorsPolicy";
        return services.AddCustomCors(config, defaultPolicyName, setupOptions, configureMoreActions);
    }

    /// <summary>
    /// CORS Origins
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static string GetCorsOrigins(
        this IConfiguration configuration,
        string configVariable = "CorsOrigins",
        string envVariable = "CORS_CONFIG")
    {
        var origins = configuration.GetValue<string>(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            origins = env;
        }

        return string.IsNullOrEmpty(origins) ? "*" : origins;
    }
}