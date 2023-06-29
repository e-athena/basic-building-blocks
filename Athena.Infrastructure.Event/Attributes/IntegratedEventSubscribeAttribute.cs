namespace Athena.Infrastructure.Event.Attributes;

/// <summary>
/// 集成事件订阅者
/// </summary>
public class IntegratedEventSubscribeAttribute : CapSubscribeAttribute
{
    /// <summary>
    /// CAP订阅者
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="isConvertChar">是否转换名称。UserCreatedEvent->user.created.event</param>
    /// <param name="isPartial"></param>
    public IntegratedEventSubscribeAttribute(string name, bool isConvertChar = true, bool isPartial = false)
        : base(isConvertChar ? StringHelper.ConvertToLowerAndAddPoint(name) : name, isPartial)
    {
    }
}