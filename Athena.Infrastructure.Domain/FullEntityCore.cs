using System.ComponentModel.DataAnnotations.Schema;
using Athena.Infrastructure.DataAnnotations.Schema;

namespace Athena.Infrastructure.Domain;

/// <summary>
/// 全功能实体
/// </summary>
[Index("is_deleted", IsUnique = false)]
[Index("tenant_id", IsUnique = false)]
[Index("tenant_id", "organizational_unit_id", IsUnique = false)]
public class FullEntityCore : EntityCore, IFullCore
{
    /// <summary>
    /// 创建人ID
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    [Column("created_user_id")]
    public string? CreatedUserId { get; set; }

    /// <summary>
    /// 更新人ID
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    [Column("last_updated_user_id")]
    public string? LastUpdatedUserId { get; set; }

    /// <summary>
    /// 是否已标记删除
    /// </summary>
    [FieldSort(999)]
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    [FieldSort(999)]
    [Column("deleted_on")]
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// 删除人
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    [Column("deleted_user_id")]
    public string? DeletedUserId { get; set; }

    /// <summary>
    /// 创建人组织架构Id
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    [Column("organizational_unit_id")]
    public string? OrganizationalUnitId { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    [MaxLength(36)]
    [FieldSort(999)]
    [Column("tenant_id")]
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