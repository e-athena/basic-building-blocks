namespace Athena.Infrastructure.Event.Attributes;

/// <summary>
/// 集成事件订阅者
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
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

    /// <summary>
    /// CAP订阅者
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <param name="group">分组名称</param>
    public IntegratedEventSubscribeAttribute(string name, string group)
        : base(StringHelper.ConvertToLowerAndAddPoint(name))
    {
        Group = $"{StringHelper.ConvertToLowerAndAddPoint(group)}.group";
    }

    /// <summary>
    /// CAP订阅者
    /// </summary>
    /// <param name="topicNameType">主题名称类型</param>
    /// <param name="group">分组名称</param>
    public IntegratedEventSubscribeAttribute(Type topicNameType, string group)
        : this(topicNameType.FullName ?? topicNameType.Name, group)
    {
    }

    /// <summary>
    /// CAP订阅者
    /// </summary>
    /// <param name="topicNameType">主题名称类型</param>
    /// <param name="groupType">分组名称</param>
    public IntegratedEventSubscribeAttribute(Type topicNameType, Type groupType)
        : this(topicNameType, groupType.FullName ?? groupType.Name)
    {
    }
}