using Athena.Infrastructure.Domain.Attributes;

namespace Athena.Infrastructure.Domain;

/// <summary>
/// 全功能实体
/// </summary>
public class FullEntityCore : EntityCore, IFullCore
{
    /// <summary>
    /// 创建人ID
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    public string? CreatedUserId { get; set; }

    /// <summary>
    /// 更新人ID
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    public string? LastUpdatedUserId { get; set; }

    /// <summary>
    /// 是否已标记删除
    /// </summary>
    [FieldSort(999)]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    [FieldSort(999)]
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// 删除人
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    public string? DeletedUserId { get; set; }

    /// <summary>
    /// 创建人组织架构Ids
    /// </summary>
    [MaxLength(-1)]
    [FieldSort(999)]
    public string? OrganizationalUnitIds { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    public string? TenantId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public FullEntityCore()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public FullEntityCore(string id) : base(id)
    {
    }
}