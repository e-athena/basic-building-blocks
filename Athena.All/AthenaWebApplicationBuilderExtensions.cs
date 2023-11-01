using System.Reflection;
using System.Security.Claims;
using Athena.Infrastructure.ApiPermission.Models;
using Athena.Infrastructure.ApiPermission.Services;
using Athena.Infrastructure.Cookies;
using Athena.Infrastructure.DataPermission;
using Athena.Infrastructure.DataPermission.Models;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.Mvc.Messaging.Requests;
using Athena.Infrastructure.Providers;
using Athena.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authentication;

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
        if (configuration != null && !configuration.GetValue<bool>("Module:Swagger:Disabled"))
        {
            app.UseCustomSwagger();
        }

        app.UseAthenaProvider();
        app.UseCustomStaticFiles();
        if (configuration != null && !configuration.GetValue<bool>("Module:Cors:Disabled"))
        {
            app.UseCors();
        }

        //启用验证
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCustomAuditLog();
        if (configuration != null &&
            !configuration.GetValue<bool>("Module:DbContext:Disabled") &&
            !configuration.GetValue<bool>("Module:DbContext:Dashboard:Disabled"))
        {
            app.UseCapDashboard();
        }

        app.UseCustomMiddlewareInjector();
        useActions?.Invoke(app);
        app.MapControllers();
        app.MapSpaFront<TType>();
        app.MapHealth();
        app.MapMenuResources<TType>();
        app.MapDatePermissionResources();
        mapActions?.Invoke(app);
        return app;
    }

    /// <summary>
    /// 读取菜单功能资源
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void MapMenuResources<TType>(this WebApplication app)
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
    private static void MapDatePermissionResources(this WebApplication app)
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

                var dataPermissionFactory = new DataPermissionFactory(services);
                var groupList = DataPermissionHelper.GetGroupList(appId);
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
        // 读取当前程序集
        var assembly = Assembly.GetExecutingAssembly();
        // 读取assemblyName
        var assemblyName = assembly.GetName().Name;
        // assemblyName
        var embeddedFileNamespace = $"{assemblyName}.wwwroot";

        app.Map("/cap/login", async httpContext =>
        {
            var tempName = "login.html";
            // 如果是post请求，读取body中的参数
            if (httpContext.Request.Method.ToLower() == "post")
            {
                // 读取body中的参数
                httpContext.Request.EnableBuffering();
                var reqStream = new StreamReader(httpContext.Request.Body);
                var reqBody = await reqStream.ReadToEndAsync();
                // httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

                // username=admin&password=123456转为LoginRequest
                var loginRequest = new LoginRequest();
                var parameters = reqBody.Split("&");
                foreach (var parameter in parameters)
                {
                    var key = parameter.Split("=")[0];
                    var value = parameter.Split("=")[1];
                    switch (key)
                    {
                        case "username":
                            loginRequest.UserName = value;
                            break;
                        case "password":
                            loginRequest.Password = value;
                            break;
                    }
                }

                // 读取配置
                var configuration = AthenaProvider.Provider?.GetService<IConfiguration>();
                if (configuration == null) throw new InvalidOperationException();
                var userName = configuration.GetValue<string>("Module:DbContext:Dashboard:UserName");
                var password = configuration.GetValue<string>("Module:DbContext:Dashboard:Password");
                userName ??= "admin";
                password ??= "admin123456";
                // 如果用户名和密码正确，则登录成功，生成Cookies，然后重定向到/cap
                if (loginRequest.UserName == userName && loginRequest.Password == password)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, loginRequest.UserName),
                        new(ClaimTypes.Name, loginRequest.UserName),
                        new(ClaimTypes.Role, "admin")
                    };
                    var claimsIdentity =
                        new ClaimsIdentity(claims, CapCookieAuthenticationDefaults.AuthenticationScheme);
                    // 读取配置中的过期时间
                    var expireMinutes = configuration.GetValue<int>("Module:DbContext:Dashboard:CookieExpires");
                    expireMinutes = expireMinutes <= 0 ? 60 : expireMinutes;
                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(expireMinutes),
                        IsPersistent = true
                    };
                    await httpContext.SignInAsync(
                        CapCookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    );
                    // 跳转到首页
                    httpContext.Response.StatusCode = 301;
                    httpContext.Response.Headers["Location"] = "/cap";
                    return;
                }

                tempName = "login_error.html";
            }

            var name = $"{embeddedFileNamespace}.{tempName}";

            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentType = "text/html;charset=utf-8";

            await using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) throw new InvalidOperationException();

            using var sr = new StreamReader(stream);
            var htmlBuilder = new StringBuilder(await sr.ReadToEndAsync());
            await httpContext.Response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
        });
    }
}