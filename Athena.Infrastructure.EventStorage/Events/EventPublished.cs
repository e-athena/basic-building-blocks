namespace Athena.Infrastructure.EventStorage.Events;

/// <summary>
/// 事件发布
/// </summary>
public class EventPublished : EventBase
{
    /// <summary>
    /// 事件发布
    /// </summary>
    /// <param name="eventStream"></param>
    public EventPublished(EventStream eventStream)
    {
        EventStream = eventStream;
    }

    /// <summary>
    /// 事件流
    /// </summary>
    public EventStream EventStream { get; }
}