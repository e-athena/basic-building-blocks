namespace Athena.InstantMessaging.Models;

/// <summary>
/// 通知消息
/// </summary>
public class NoticeMessage
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 目标地址
    /// </summary>
    public string? TargetUrl { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public string? Sender { get; set; }
}