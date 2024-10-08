using Athena.Infrastructure.ApiPermission.Models;
using Athena.Infrastructure.ApiPermission.Services;
using Athena.Infrastructure.DataPermission;
using Athena.Infrastructure.DataPermission.Models;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.Providers;
using Athena.Infrastructure.ViewModels;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 
/// </summary>
public static class AthenaWebApplicationBuilderExtensions
{
    /// <summary>
    /// Runs an application and block the calling thread until host shutdown.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="mapActions"></param>
    /// <param name="useActions"></param>
    /// <typeparam name="TType"></typeparam>
    public static WebApplication UseCustomAthena<TType>(this WebApplication app,
        Action<WebApplication>? useActions = null,
        Action<WebApplication>? mapActions = null)
    {
        var configuration = app.Services.GetService<IConfiguration>();
        // Configure the HTTP request pipeline.
        if (configuration != null && !configuration.GetEnvValue<bool>("Module:Swagger:Disabled"))
        {
            app.UseCustomSwagger();
        }

        app.UseAthenaProvider();
        app.UseCustomStaticFiles();
        if (configuration != null && !configuration.GetEnvValue<bool>("Module:Cors:Disabled"))
        {
            app.UseCors();
        }

        //启用验证
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCustomAuditLog();
        if (configuration != null &&
            !configuration.GetEnvValue<bool>("Module:DbContext:Disabled") &&
            !configuration.GetEnvValue<bool>("Module:DbContext:Dashboard:Disabled"))
        {
            app.UseCapDashboard();
        }

        app.UseCustomMiddlewareInjector();
        useActions?.Invoke(app);
        app.MapControllers();
        app.MapSpaFront<TType>();
        app.MapHealth();
        app.MapMenuResources<TType>();
        app.MapDataPermissionResources();
        mapActions?.Invoke(app);
        return app;
    }

    /// <summary>
    /// 读取菜单功能资源
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void MapMenuResources<TType>(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/external/get-menu-resources", () =>
            {
                var service = AthenaProvider.Provider?.GetService<IApiPermissionService>();
                if (service == null)
                {
                    return new ApiResult<IList<MenuTreeInfo>>
                    {
                        Success = true,
                        Data = new List<MenuTreeInfo>()
                    };
                }

                // 读取服务名
                var appId = AthenaProvider.Provider?.GetService<IConfiguration>()?.GetValue<string>("ServiceName");
                // TType is the type of the current assembly
                var assembly = typeof(TType).Assembly;
                return new ApiResult<IList<MenuTreeInfo>>
                {
                    Success = true,
                    Data = service.GetFrontEndRoutingResources(assembly, appId ?? "unknown")
                };
            })
            .AddEndpointFilter<BasicAuthEndpointFilter>();
    }

    /// <summary>
    /// 读取数据权限资源
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void MapDataPermissionResources(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/external/get-data-permission-resources", () =>
            {
                // 读取服务名
                var appId = AthenaProvider.Provider?.GetService<IConfiguration>()?.GetValue<string>("ServiceName") ??
                            "unknown";
                var rsp = new ApiResult<ApplicationDataPermissionInfo>
                {
                    Success = true,
                    Data = new ApplicationDataPermissionInfo
                    {
                        ApplicationId = appId,
                        ApplicationName = appId,
                        DataPermissionGroups = new List<DataPermissionGroup>(),
                        ExtraSelectList = new List<SelectViewModel>()
                    }
                };
                var services = AthenaProvider.Provider?.GetService<IEnumerable<IDataPermission>>();
                if (services == null)
                {
                    return rsp;
                }

                //
                var dataPermissionStaticService = AthenaProvider.Provider?.GetService<IDataPermissionStaticService>();

                var dataPermissionFactory = new DataPermissionFactory(services);
                var groupList = dataPermissionStaticService == null
                    ? DataPermissionHelper.GetGroupList(appId)
                    : dataPermissionStaticService.GetGroupList(appId);
                rsp.Data = new ApplicationDataPermissionInfo
                {
                    ApplicationId = appId,
                    ApplicationName = appId,
                    DataPermissionGroups = groupList,
                    ExtraSelectList = dataPermissionFactory.GetSelectList()
                };
                return rsp;
            })
            .AddEndpointFilter<BasicAuthEndpointFilter>();
    }

    /// <summary>
    /// Cap Dashboard
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void UseCapDashboard(this WebApplication app)
    {
        // 读取配置
        var configuration = app.Services.GetService<IConfiguration>();

        if (configuration == null)
        {
            return;
        }
        // 读取配置
        var capDashboardOptions = app.Services.GetService<IOptionsMonitor<DashboardOptions>>();
        // 如果是访问/cap页面，则判断是否登录，如果未登录，则使用Basic Auth登录
        app.Use(async (context, next) =>
        {
            // cap dashboard
            if (context.Request.Path.ToString().StartsWith(capDashboardOptions?.CurrentValue.PathMatch ?? "/cap"))
            {
                var isDisabledAuth = configuration.GetEnvValue<bool>("Module:DbContext:Dashboard:DisabledAuth");
                // 需要授权访问
                if (!isDisabledAuth)
                {
                    var header = (string)context.Request.Headers["Authorization"]!;
                    if (!string.IsNullOrWhiteSpace(header))
                    {
                        var authenticationHeaderValue = AuthenticationHeaderValue.Parse(header);
                        if ("Basic".Equals(authenticationHeaderValue.Scheme, StringComparison.OrdinalIgnoreCase))
                        {
                            var strArray = Encoding.UTF8
                                .GetString(Convert.FromBase64String(authenticationHeaderValue.Parameter!)).Split(':');
                            if (strArray.Length > 1)
                            {
                                var login = strArray[0];
                                var pwd = strArray[1];
                                var userName = configuration.GetEnvValue<string>("Module:DbContext:Dashboard:UserName");
                                var password = configuration.GetEnvValue<string>("Module:DbContext:Dashboard:Password");
                                userName ??= "admin";
                                password ??= "admin123456";

                                // 如果用户名和密码正确，则登录成功，生成Cookies，然后重定向到/cap
                                if (login == userName && pwd == password)
                                {
                                    await next();
                                    return;
                                }
                            }
                        }
                    }

                    context.Response.StatusCode = 401;
                    context.Response.Headers.Append("WWW-Authenticate", (StringValues)"Basic realm=\"CAP Dashboard\"");
                    return;
                }
            }

            await next();
        });
    }
}