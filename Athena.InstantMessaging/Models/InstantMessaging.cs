namespace Athena.InstantMessaging.Models;

/// <summary>
/// 实时消息
/// </summary>
public class InstantMessaging<TData>
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public string? NoticeType { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public string? MessageType { get; set; }

    /// <summary>
    /// 数据体
    /// </summary>
    public TData? Data { get; set; }
}

/// <summary>
/// 消息类型
/// </summary>
public enum MessageType
{
    /// <summary>
    /// 通知
    /// </summary>
    Notice = 0,

    /// <summary>
    /// 事件
    /// </summary>
    Event = 1,

    /// <summary>
    /// 消息
    /// </summary>
    Message = 2
}