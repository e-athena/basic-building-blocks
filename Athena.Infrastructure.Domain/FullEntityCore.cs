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
    public string? CreatedUserId { get; set; }

    /// <summary>
    /// 更新人ID
    /// </summary>
    [MaxLength(36)]
    public string? LastUpdatedUserId { get; set; }

    /// <summary>
    /// 是否已标记删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedOn { get; set; }

    /// <summary>
    /// 删除人
    /// </summary>
    [MaxLength(36)]
    public string? DeletedUserId { get; set; }

    /// <summary>
    /// 创建人组织架构Ids
    /// </summary>
    [MaxLength(-1)]
    public string? OrganizationalUnitIds { get; set; }

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