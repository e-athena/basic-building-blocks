namespace Athena.Infrastructure.Domain;

/// <summary>
/// 组织/部门
/// </summary>
public interface IOrganization
{
    /// <summary>
    /// 组织机构Id
    /// </summary>
    public string? OrganizationalUnitId { get; set; }
}