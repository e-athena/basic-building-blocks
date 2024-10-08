namespace Athena.Infrastructure.EventStorage.Models;

/// <summary>
/// 事件源
/// </summary>
[Table("event_streams")]
[Index("user_id")]
[Index("version")]
[Index("created_on")]
[Index("aggregate_root_id")]
public class EventStream
{
    /// <summary>
    /// 自增ID
    /// </summary>
    /// <value></value>
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("sequence")]
    public long Sequence { get; set; }

    /// <summary>
    /// 聚合根类型名称
    /// </summary>
    /// <value></value>
    [MaxLength(256)]
    [Required]
    [Column("aggregate_root_type_name")]
    public string AggregateRootTypeName { get; set; } = null!;

    /// <summary>
    /// 聚合根ID
    /// </summary>
    /// <value></value>
    [MaxLength(36)]
    [Required]
    [Column("aggregate_root_id")]
    public string AggregateRootId { get; set; } = null!;

    /// <summary>
    /// 版本号
    /// </summary>
    /// <value></value>
    [Required]
    [Column("version")]
    public int Version { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    /// <value></value>
    [MaxLength(36)]
    [Column("event_id")]
    public string EventId { get; set; } = null!;

    /// <summary>
    /// 事件名称
    /// </summary>
    [Column("event_name")]
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    /// <value></value>
    [Required]
    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// 事件
    /// </summary>
    /// <value></value>
    [Required]
    [MaxLength(-1)]
    [Column("events")]
    public string Events { get; set; } = null!;

    /// <summary>
    /// 用户Id
    /// </summary>
    [MaxLength(36)]
    [Column("user_id")]
    public string? UserId { get; set; }
}