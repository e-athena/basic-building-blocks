namespace Athena.Infrastructure.Tenants;

/// <summary>
/// 租户服务接口
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// 读取租户信息
    /// </summary>
    /// <param name="tenantCode">租户编码</param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    Task<TenantInfo?> GetAsync(string tenantCode, string? appId);
}