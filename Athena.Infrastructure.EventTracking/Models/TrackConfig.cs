namespace Athena.Infrastructure.EventTracking.Models;

/// <summary>
/// 追踪配置
/// </summary>
[Table("event_tracking_track_configs")]
[Index(nameof(EventTypeFullName))]
[Index(nameof(ConfigId))]
public class TrackConfig : EntityBase
{
    /// <summary>
    /// 配置ID
    /// <remarks>为空时为根配置节点</remarks>
    /// </summary>
    [MaxLength(36)]
    public string? ConfigId { get; set; }

    /// <summary>
    /// 上级Id
    /// <remarks>为空时为根节点</remarks>
    /// </summary>
    [MaxLength(36)]
    public string? ParentId { get; set; }

    /// <summary>
    /// 上级路径
    /// </summary>
    public string? ParentPath { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public EventType EventType { get; set; }

    /// <summary>
    /// 事件名
    /// </summary>
    [MaxLength(64)]
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 事件类型名
    /// </summary>
    [MaxLength(128)]
    public string EventTypeName { get; set; } = null!;

    /// <summary>
    /// 事件类型全名
    /// </summary>
    [MaxLength(128)]
    public string EventTypeFullName { get; set; } = null!;

    /// <summary>
    /// 处理器名
    /// </summary>
    [MaxLength(64)]
    public string? ProcessorName { get; set; }

    /// <summary>
    /// 处理器全名
    /// </summary>
    [MaxLength(128)]
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