namespace Athena.InstantMessaging.Models;

/// <summary>
/// 日程消息
/// </summary>
public class ScheduleMessage
{
    /// <summary>
    /// 发送者
    /// </summary>
    public string? Sender { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 详情
    /// </summary>
    public string? Description { get; set; }
}