namespace Athena.Infrastructure.Domain;

/// <summary>
/// 租户接口
/// </summary>
public interface ITenant
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public string? TenantId { get; set; }
}