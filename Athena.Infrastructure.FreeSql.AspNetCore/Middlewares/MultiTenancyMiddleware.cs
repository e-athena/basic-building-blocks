namespace Athena.Infrastructure.FreeSql.AspNetCore.Middlewares;

/// <summary>
/// 多租户中件间
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class MultiTenancyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MultiTenancyMiddleware> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="loggerFactory"></param>
    public MultiTenancyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<MultiTenancyMiddleware>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.HasValue || !context.Request.Path.Value.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        try
        {
            FreeSqlMultiTenancyManager.Instance.DistributeTrace ??= log =>
            {
                // 写日志
                _logger.LogDebug("{Msg}", log.Split('\n')[0].Trim());
            };

            //通过context的请求头或参数中读取租户code
            var tenantKey = GetTenantKey(context);

            if (string.IsNullOrEmpty(tenantKey))
            {
                // 切换为主租户
                FreeSqlMultiTenancyManager.Instance.Change(Constant.DefaultMainTenant);
                await _next(context);
                return;
            }

            var tenantService = context.RequestServices.GetService<ITenantService>();
            var configuration = context.RequestServices.GetService<IConfiguration>();
            if (tenantService == null && configuration != null)
            {
                tenantService = new DefaultTenantService(configuration);
            }

            if (tenantService == null)
            {
                // 切换为主租户
                FreeSqlMultiTenancyManager.Instance.Change(Constant.DefaultMainTenant);
                await _next(context);
                return;
            }

            // 读取租户信息
            var tenant = await tenantService.GetAsync(tenantKey, GetAppId(context));
            // 租户不存在或者为共享租户
            if (tenant == null || tenant.IsolationLevel == TenantIsolationLevel.Shared)
            {
                // 切换为主租户
                FreeSqlMultiTenancyManager.Instance.Change(Constant.DefaultMainTenant);
                await _next(context);
                return;
            }

            // 设置当前租户
            var currentTenant = tenant.DbKey;
            // 是否自动同步结构
            var isAutoSyncStructure = configuration != null &&
                                      configuration.GetValue<bool>("Module:DbContext:IsAutoSyncStructure");
            // 注册租户
            // 只会首次注册，如果已经注册过则不生效
            FreeSqlMultiTenancyManager.Instance.Register(currentTenant, () =>
                FreeSqlBuilderHelper.Build(
                    tenant.ConnectionString,
                    tenant.DataType.HasValue ? (DataType) tenant.DataType.Value : null,
                    isAutoSyncStructure
                )
            );

            // 切换租户
            FreeSqlMultiTenancyManager.Instance.Change(currentTenant);
            await _next(context);
        }
        finally
        {
            // 切换为主租户
            FreeSqlMultiTenancyManager.Instance.Change(Constant.DefaultMainTenant);
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