namespace Athena.Infrastructure.EventStorage.Models;

/// <summary>
/// 事件源
/// </summary>
[Table("event_streams")]
[Index(nameof(UserId))]
[Index(nameof(Version))]
[Index(nameof(CreatedOn))]
[Index(nameof(AggregateRootId))]
public class EventStream
{
    /// <summary>
    /// 自增ID
    /// </summary>
    /// <value></value>
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Sequence { get; set; }

    /// <summary>
    /// 聚合根类型名称
    /// </summary>
    /// <value></value>
    [MaxLength(256)]
    [Required]
    public string AggregateRootTypeName { get; set; } = null!;

    /// <summary>
    /// 聚合根ID
    /// </summary>
    /// <value></value>
    [MaxLength(36)]
    [Required]
    public string AggregateRootId { get; set; } = null!;

    /// <summary>
    /// 版本号
    /// </summary>
    /// <value></value>
    [Required]
    public int Version { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    /// <value></value>
    public string EventId { get; set; } = null!;

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    /// <value></value>
    [Required]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// 事件
    /// </summary>
    /// <value></value>
    [Required]
    [MaxLength(-1)]
    public string Events { get; set; } = null!;

    /// <summary>
    /// 用户Id
    /// </summary>
    [MaxLength(36)]
    public string? UserId { get; set; }
}