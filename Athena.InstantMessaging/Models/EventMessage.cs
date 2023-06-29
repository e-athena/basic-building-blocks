namespace Athena.InstantMessaging.Models;

/// <summary>
/// 事件消息
/// </summary>
public class EventMessage<T>
{
    /// <summary>
    /// 类型
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string SpecVersion { get; set; } = "1.0";

    /// <summary>
    /// 源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///  时间
    /// </summary>
    public string Time { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }
}