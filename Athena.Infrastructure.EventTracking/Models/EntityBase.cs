namespace Athena.Infrastructure.EventTracking.Models;

/// <summary>
/// 实体基类
/// </summary>
public class EntityBase
{
    /// <summary>
    /// ID
    /// </summary>
    [Required]
    [MaxLength(36)]
    [Key]
    public string Id { get; set; } = ObjectId.GenerateNewStringId();

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.Now;

}