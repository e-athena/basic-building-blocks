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
    public static WebApplicationBuilder AddAthena(
        this WebApplicationBuilder builder,
        Action<IServiceCollection>? moreServiceActions = null
    )
    {
        var configuration = builder.Configuration;
        builder.AddCustomAthena(services =>
        {
            if (!configuration.GetEnvValue<bool>("Module:DbContext:Disabled"))
            {
                // 添加ORM
                services.AddCustomSqlSugar(configuration);
                // 添加集成事件支持
                services.AddCustomIntegrationEvent(configuration, capOptions =>
                {
                    // 启用仪表盘
                    if (!configuration.GetEnvValue<bool>("Module:DbContext:Dashboard:Disabled"))
                    {
                        // Dashboard
                        capOptions.UseDashboard(options =>
                        {
                            options.UseAuth = !configuration.GetEnvValue<bool>("Module:DbContext:Dashboard:DisabledAuth");
                            options.DefaultAuthenticationScheme = CapCookieAuthenticationDefaults.AuthenticationScheme;
                            options.AuthorizationPolicy = CapCookieAuthenticationDefaults.AuthenticationScheme;
                        });
                    }
                });
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
        });
        return builder;
    }
}