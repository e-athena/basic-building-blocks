namespace Athena.Infrastructure.DataPermission.Models;

/// <summary>
/// 组织架构数据权限
/// </summary>
public class OrganizationalUnitAuth
{
    /// <summary>
    /// 组织ID
    /// </summary>
    public string OrganizationalUnitId { get; set; } = null!;

    /// <summary>
    /// 业务ID
    /// </summary>
    public string BusinessId { get; set; } = null!;

    /// <summary>
    /// 业务表
    /// </summary>
    public string BusinessTable { get; set; } = null!;
}