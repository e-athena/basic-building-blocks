namespace Athena.Infrastructure.Domain;

/// <summary>
/// 软删除
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// 是否已删除
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
}