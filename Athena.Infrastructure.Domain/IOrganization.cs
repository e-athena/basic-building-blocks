namespace Athena.Infrastructure.Domain;

/// <summary>
/// 组织/部门
/// </summary>
public interface IOrganization
{
    /// <summary>
    /// 所属组织机构Ids
    /// <remarks>多个用逗号分割</remarks>
    /// </summary>
    public string? OrganizationalUnitIds { get; set; }
}