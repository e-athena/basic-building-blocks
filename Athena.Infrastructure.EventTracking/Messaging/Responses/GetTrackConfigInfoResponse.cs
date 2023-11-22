namespace Athena.Infrastructure.EventTracking.Messaging.Responses;

/// <summary>
/// 读取详情
/// </summary>
public class GetTrackConfigInfoResponse : TrackConfigModel
{
    /// <summary>
    /// 事件类型名称
    /// </summary>
    public string EventTypeTitle => EventType.ToDescription();

    /// <summary>
    /// 子节点
    /// </summary>
    public List<GetTrackConfigInfoResponse>? Children { get; set; }
}