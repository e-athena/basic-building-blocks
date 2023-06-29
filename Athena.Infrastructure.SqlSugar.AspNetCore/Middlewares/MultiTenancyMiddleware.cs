using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Infrastructure.SqlSugar.AspNetCore.Middlewares;

/// <summary>
/// 多租户中件间
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class MultiTenancyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MultiTenancyMiddleware> _logger;
    private readonly ISqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="sqlSugarClient"></param>
    public MultiTenancyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, ISqlSugarClient sqlSugarClient)
    {
        _next = next;
        _sqlSugarClient = sqlSugarClient;
        _logger = loggerFactory.CreateLogger<MultiTenancyMiddleware>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.Value.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        try
        {
            //通过context的请求头或参数中读取租户code
            var tenantKey = GetTenantKey(context);
            if (string.IsNullOrEmpty(tenantKey))
            {
                _sqlSugarClient.AsTenant().ChangeDatabase(Constant.DefaultMainTenant);
                await _next(context);
                return;
            }

            var tenantService = context.RequestServices.GetService<ITenantService>();
            if (tenantService == null)
            {
                var configuration = context.RequestServices.GetService<IConfiguration>();
                if (configuration != null)
                {
                    tenantService = new DefaultTenantService(configuration);
                }
            }

            if (tenantService == null)
            {
                _sqlSugarClient.AsTenant().ChangeDatabase(Constant.DefaultMainTenant);
                await _next(context);
                return;
            }

            // 读取租户信息
            var tenant = await tenantService.GetAsync(tenantKey, GetAppId(context));
            if (tenant == null)
            {
                _sqlSugarClient.AsTenant().ChangeDatabase(Constant.DefaultMainTenant);
                await _next(context);
                return;
            }

            // 设置当前租户
            var currentTenant = tenant.DbKey;

            // 如果当前租户已经存在，则直接切换
            var exists = _sqlSugarClient.AsTenant().IsAnyConnection(currentTenant);
            if (exists)
            {
                _sqlSugarClient.AsTenant().ChangeDatabase(currentTenant);
                await _next(context);
                return;
            }

            // 注册租户
            SqlSugarBuilderHelper.Registry(
                _sqlSugarClient,
                tenant.DbKey,
                tenant.ConnectionString,
                tenant.DataType.HasValue ? (DbType) tenant.DataType.Value : null
            );
            _sqlSugarClient.AsTenant().ChangeDatabase(currentTenant);
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "多租户中间件异常");
            throw;
        }
        finally
        {
            _sqlSugarClient.AsTenant().ChangeDatabase(Constant.DefaultMainTenant);
        }
    }

    /// <summary>
    /// 读取租户ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static string? GetTenantKey(HttpContext context)
    {
        var tenantId = context.Request.Headers["TenantId"];
        if (!string.IsNullOrEmpty(tenantId))
        {
            return tenantId;
        }

        tenantId = context.Request.Query["tenant_id"].ToString();
        return (!string.IsNullOrEmpty(tenantId)
            ? tenantId.ToString()
            : context.User.FindFirst("TenantId")?.Value) ?? null;
    }

    /// <summary>
    /// 读取应用Id
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static string? GetAppId(HttpContext context)
    {
        var appId = context.Request.Headers["AppId"];
        if (!string.IsNullOrEmpty(appId))
        {
            return appId;
        }

        appId = context.Request.Query["app_id"].ToString();
        return (!string.IsNullOrEmpty(appId)
            ? appId.ToString()
            : context.User.FindFirst("AppId")?.Value) ?? null;
    }
}