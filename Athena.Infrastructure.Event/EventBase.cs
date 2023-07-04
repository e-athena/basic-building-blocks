using Athena.Infrastructure.Event.DomainEvents;
using Athena.Infrastructure.Event.IntegrationEvents;

namespace Athena.Infrastructure.Event;

/// <summary>
/// 事件基类
/// </summary>
public abstract class EventBase : IDomainEvent, IIntegrationEvent
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    /// <summary>
    /// 元数据
    /// </summary>
    public IDictionary<string, object> MetaData { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// 读取ID
    /// </summary>
    /// <returns></returns>
    public string? GetId()
    {
        var id = MetaData.FirstOrDefault(p => p.Key == "id");
        return id.Value.ToString();
    }

    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; set; } = ObjectId.GenerateNewStringId();

    /// <summary>
    /// 根跟踪Id
    /// </summary>
    public string? RootTraceId { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// CAP Callback subscriber name
    /// </summary>
    public string? CallbackName { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    protected EventBase()
    {
        EventName = StringHelper.ConvertToLowerAndAddPoint(GetType().Name);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventName">事件名</param>
    protected EventBase(string eventName)
    {
        EventName = eventName;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="callbackName">Callback subscriber name</param>
    protected EventBase(string eventName, string? callbackName)
    {
        EventName = eventName;
        CallbackName = callbackName;
    }
}