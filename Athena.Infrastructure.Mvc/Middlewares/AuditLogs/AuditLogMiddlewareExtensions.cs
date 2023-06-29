using Athena.Infrastructure.Mvc.Middlewares.AuditLogs;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 审计日志中间件扩展类
/// </summary>
public static class AuditLogMiddlewareExtensions
{
    /// <summary>
    /// 使用审计日志
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomAuditLog(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuditLogMiddleware>();
    }
}