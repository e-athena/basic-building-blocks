namespace Athena.InstantMessaging.Models;

/// <summary>
/// 待办事件消息
/// </summary>
public class TodoEventMessage
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 扩展信息
    /// </summary>
    public string? Extra { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 目标地址
    /// </summary>
    public string? TargetUrl { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public string? Sender { get; set; }
}