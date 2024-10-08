using DotNetCore.CAP;
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
    /// <param name="actionFreeSqlAop"></param>
    /// <param name="actionFreeSqlBuilder"></param>
    /// <param name="actionCapOptions"></param>
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
    public static WebApplicationBuilder AddAthena(
        this WebApplicationBuilder builder,
        Action<IServiceCollection>? moreServiceActions = null,
        // FreeSql相关配置
        Action<IAop>? actionFreeSqlAop = null,
        Action<FreeSqlBuilder>? actionFreeSqlBuilder = null,
        Action<CapOptions>? actionCapOptions = null,
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
        var configuration = builder.Configuration;
        builder.AddCustomAthena(services =>
            {
                if (!configuration.GetEnvValue<bool>("Module:DbContext:Disabled"))
                {
                    // 是否自动同步结构
                    var isAutoSyncStructure = configuration.GetEnvValue<bool>("Module:DbContext:IsAutoSyncStructure");
                    // 添加ORM
                    services.AddCustomFreeSql(configuration, isAutoSyncStructure, actionFreeSqlAop,
                        actionFreeSqlBuilder);
                    // 添加集成事件支持
                    services.AddCustomIntegrationEvent(configuration, capOptions =>
                    {
                        // 启用仪表盘
                        if (!configuration.GetEnvValue<bool>("Module:DbContext:Dashboard:Disabled"))
                        {
                            // Dashboard
                            capOptions.UseDashboard();
                        }

                        actionCapOptions?.Invoke(capOptions);
                    });
                }

                if (!configuration.GetEnvValue<bool>("Module:EventStorage:Disabled"))
                {
                    services.AddCustomEventStorage(configuration);
                }

                if (!configuration.GetEnvValue<bool>("Module:DataPermission:Disabled"))
                {
                    services.AddCustomDataPermission(configuration);
                }

                if (!configuration.GetEnvValue<bool>("Module:Logger:Disabled"))
                {
                    services.AddCustomStorageLogger(configuration);
                }

                if (!configuration.GetEnvValue<bool>("Module:EventTracking:Disabled"))
                {
                    services.AddCustomEventTracking(configuration);
                }

                moreServiceActions?.Invoke(services);
            },
            actionSwaggerGenOptions,
            actionJwtBearerOptions,
            actionAuthorizationOptions,
            actionAuthenticationBuilder,
            actionSignalrHubOptions,
            actionSignalrRedisOptions,
            actionCorsOptions,
            actionMvcOptions,
            actionSerilogConfigureLogger,
            serilogPreserveStaticLogger,
            serilogWriteToProviders
        );
        return builder;
    }
}