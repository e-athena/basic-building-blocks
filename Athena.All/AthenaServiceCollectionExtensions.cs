using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Builder;

public static class AthenaServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="moreServiceActions"></param>
    /// <param name="actionSwaggerGenOptions"></param>
    /// <param name="actionJwtBearerOptions"></param>
    /// <param name="actionAuthorizationOptions"></param>
    /// <param name="actionAuthenticationBuilder"></param>
    /// <param name="actionSignalrHubOptions"></param>
    /// <param name="actionSignalrRedisOptions"></param>
    /// <param name="actionCorsOptions"></param>
    /// <param name="actionMvcOptions"></param>
    /// <param name="actionSerilogConfigureLogger"></param>
    /// <param name="serilogPreserveStaticLogger"></param>
    /// <param name="serilogWriteToProviders"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddCustomAthena(
        this WebApplicationBuilder builder,
        Action<IServiceCollection>? moreServiceActions = null,
        // swagger配置
        Action<SwaggerGenOptions>? actionSwaggerGenOptions = null,
        // auth配置
        Action<JwtBearerOptions>? actionJwtBearerOptions = null,
        Action<AuthorizationOptions>? actionAuthorizationOptions = null,
        Action<AuthenticationBuilder>? actionAuthenticationBuilder = null,
        // signalR配置
        Action<HubOptions>? actionSignalrHubOptions = null,
        Action<RedisOptions>? actionSignalrRedisOptions = null,
        // cors配置
        Action<CorsOptions>? actionCorsOptions = null,
        // mvc配置
        Action<MvcOptions>? actionMvcOptions = null,
        // serilog logger配置
        Action<HostBuilderContext, LoggerConfiguration>? actionSerilogConfigureLogger = null,
        bool serilogPreserveStaticLogger = false,
        bool serilogWriteToProviders = false
    )
    {
        SelfLog.Enable(Console.Error);
        var configuration = builder.Configuration;
        var services = builder.Services;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // Add services to the container.
        services.AddOptions();
        services.AddAthenaProvider();
        services.AddCustomServiceComponent(configuration);
        services.AddCustomValidators(configuration);
        services.AddCustomMediatR(configuration);

        if (!configuration.GetEnvValue<bool>("Module:Swagger:Disabled"))
        {
            services.AddCustomSwaggerGen(configuration, actionSwaggerGenOptions);
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
            services.AddCustomAuth(
                configuration,
                actionJwtBearerOptions,
                actionAuthorizationOptions,
                actionAuthenticationBuilder
            );
        }

        if (!configuration.GetEnvValue<bool>("Module:SignalR:Disabled"))
        {
            services.AddCustomSignalRWithRedis(configuration, actionSignalrHubOptions, actionSignalrRedisOptions);
        }

        if (!configuration.GetEnvValue<bool>("Module:Cors:Disabled"))
        {
            services.AddCustomCors(configuration, actionCorsOptions);
        }

        services.AddCustomMiddlewareInjector();
        moreServiceActions?.Invoke(services);
        services
            .AddCustomController(actionMvcOptions)
            .AddNewtonsoftJson();

        builder.UseCustomSerilog(actionSerilogConfigureLogger, serilogPreserveStaticLogger, serilogWriteToProviders);
        return builder;
    }
}