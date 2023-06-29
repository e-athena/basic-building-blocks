namespace Athena.Infrastructure.Mvc.Helpers;

/// <summary>
/// 租户帮助在
/// </summary>
public static class TenantHelper
{
    /// <summary>
    /// 是否为租户环境
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool IsTenantEnvironment(HttpContext context)
    {
        return !string.IsNullOrEmpty(GetTenantCode(context));
    }

    /// <summary>
    /// 读取租户编码
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string? GetTenantCode(HttpContext context)
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
}