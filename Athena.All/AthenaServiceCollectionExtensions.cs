// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Builder;

public static class AthenaServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="moreServiceActions"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddCustomAthena(
        this WebApplicationBuilder builder,
        Action<IServiceCollection>? moreServiceActions = null
    )
    {
        SelfLog.Enable(Console.Error);
        var configuration = builder.Configuration;
        var services = builder.Services;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // Add services to the container.
        services.AddAthenaProvider();
        services.AddCustomServiceComponent(configuration);
        services.AddCustomValidators(configuration);
        services.AddCustomMediatR(configuration);

        if (!configuration.GetEnvValue<bool>("Module:Swagger:Disabled"))
        {
            services.AddCustomSwaggerGen(configuration);
        }

        if (!configuration.GetEnvValue<bool>("Module:Redis:Disabled"))
        {
            services.AddCustomCsRedisCache(configuration);
        }

        if (!configuration.GetEnvValue<bool>("Module:ApiPermission:Disabled"))
        {
            services.AddCustomApiPermission();
        }

        if (!configuration.GetEnvValue<bool>("Module:BasicAuth:Disabled"))
        {
            services.AddCustomBasicAuth(configuration);
        }

        if (!configuration.GetEnvValue<bool>("Module:Auth:Disabled"))
        {
            services.AddCustomAuth(configuration);
        }

        if (!configuration.GetEnvValue<bool>("Module:SignalR:Disabled"))
        {
            services.AddCustomSignalRWithRedis(configuration);
        }

        if (!configuration.GetEnvValue<bool>("Module:Cors:Disabled"))
        {
            services.AddCustomCors(configuration);
        }

        services.AddCustomMiddlewareInjector();
        moreServiceActions?.Invoke(services);
        services
            .AddCustomController()
            .AddNewtonsoftJson();

        builder.UseCustomSerilog();
        builder.Host.UseDefaultServiceProvider(options =>
        {
            // 
            options.ValidateScopes = false;
        });
        return builder;
    }
}