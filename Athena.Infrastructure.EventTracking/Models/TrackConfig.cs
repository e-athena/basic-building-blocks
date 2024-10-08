namespace Athena.Infrastructure.EventTracking.Models;

/// <summary>
/// 追踪配置
/// </summary>
[Table("event_tracking_track_configs")]
[Index("event_type_full_name")]
[Index("config_id")]
public class TrackConfig : EntityBase
{
    /// <summary>
    /// 配置ID
    /// <remarks>为空时为根配置节点</remarks>
    /// </summary>
    [MaxLength(36)]
    [Column("config_id")]
    public string? ConfigId { get; set; }

    /// <summary>
    /// 上级Id
    /// <remarks>为空时为根节点</remarks>
    /// </summary>
    [MaxLength(36)]
    [Column("parent_id")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 上级路径
    /// </summary>
    [Column("parent_path")]
    public string? ParentPath { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [Column("event_type")]
    public EventType EventType { get; set; }

    /// <summary>
    /// 事件名
    /// </summary>
    [MaxLength(64)]
    [Column("event_name")]
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 事件类型名
    /// </summary>
    [MaxLength(128)]
    [Column("event_type_name")]
    public string EventTypeName { get; set; } = null!;

    /// <summary>
    /// 事件类型全名
    /// </summary>
    [MaxLength(128)]
    [Column("event_type_full_name")]
    public string EventTypeFullName { get; set; } = null!;

    /// <summary>
    /// 处理器名
    /// </summary>
    [MaxLength(64)]
    [Column("processor_name")]
    public string? ProcessorName { get; set; }

    /// <summary>
    /// 处理器全名
    /// </summary>
    [MaxLength(128)]
    [Column("processor_full_name")]
    public string? ProcessorFullName { get; set; }

    /// <summary>
    /// 数据验证
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void Check()
    {
        if (ParentId != null && (ProcessorName == null || ProcessorFullName == null))
        {
            throw new ArgumentException("子节点必须指定处理器");
        }
    }
}