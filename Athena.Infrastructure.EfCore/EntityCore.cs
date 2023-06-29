namespace Athena.Infrastructure.EfCore;

/// <summary>
/// Entity基类
/// </summary>
public abstract class EntityCore
{
    /// <summary>
    /// 聚合根ID
    /// </summary>
    [Required]
    [MaxLength(36)]
    [Key]
    public string Id { get; set; } = ObjectId.GenerateNewStringId();

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Required]
    public DateTime UpdatedOn { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [Required]
    public long Version { get; set; }
}