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
        services.AddCustomServiceComponent();
        if (!configuration.GetValue<bool>("Module:Swagger:Disabled"))
        {
            services.AddCustomSwaggerGen(configuration);
        }

        if (!configuration.GetValue<bool>("Module:Redis:Disabled"))
        {
            services.AddCustomCsRedisCache(configuration);
        }

        if (!configuration.GetValue<bool>("Module:ApiPermission:Disabled"))
        {
            services.AddCustomApiPermission();
        }

        if (!configuration.GetValue<bool>("Module:BasicAuth:Disabled"))
        {
            services.AddCustomBasicAuth(configuration);
        }

        if (!configuration.GetValue<bool>("Module:JwtAuth:Disabled"))
        {
            services.AddCustomJwtAuthWithSignalR(configuration);
        }

        if (!configuration.GetValue<bool>("Module:SignalR:Disabled"))
        {
            services.AddCustomSignalRWithRedis(configuration);
        }

        if (!configuration.GetValue<bool>("Module:Cors:Disabled"))
        {
            services.AddCustomCors(configuration);
        }

        services.AddCustomController().AddNewtonsoftJson();
        moreServiceActions?.Invoke(services);

        builder.UseCustomSerilog();
        return builder;
    }
}