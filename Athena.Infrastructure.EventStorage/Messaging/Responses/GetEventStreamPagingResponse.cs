namespace Athena.Infrastructure.EventStorage.Messaging.Responses;

/// <summary>
///
/// </summary>
public class GetEventStreamPagingResponse
{
    /// <summary>
    /// 自增ID
    /// </summary>
    /// <value></value>
    public long Sequence { get; set; }

    /// <summary>
    /// 聚合根类型名称
    /// </summary>
    /// <value></value>
    public string AggregateRootTypeName { get; set; } = null!;

    /// <summary>
    /// 聚合根ID
    /// </summary>
    /// <value></value>
    public string AggregateRootId { get; set; } = null!;

    /// <summary>
    /// 版本号
    /// </summary>
    /// <value></value>
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
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    /// <value></value>
    public string? UserId { get; set; }
}