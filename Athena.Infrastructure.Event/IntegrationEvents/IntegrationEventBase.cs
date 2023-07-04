namespace Athena.Infrastructure.Event.IntegrationEvents;

/// <summary>
/// 集成事件基类
/// <remarks>用于进程外通讯</remarks>
/// </summary>
public abstract class IntegrationEventBase : IIntegrationEvent
{
    /// <summary>
    /// 租户Id
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 应用Id
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// Callback subscriber name
    /// <para>REF: https://cap.dotnetcore.xyz/user-guide/zh/cap/messaging/#_3</para>
    /// </summary>
    public string? CallbackName { get; set; }

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
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 根追踪ID
    /// </summary>
    public string? RootTraceId { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    protected IntegrationEventBase()
    {
        EventName = StringHelper.ConvertToLowerAndAddPoint(GetType().Name);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventName">事件名</param>
    protected IntegrationEventBase(string eventName)
    {
        EventName = eventName;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="callbackName">Callback subscriber name</param>
    protected IntegrationEventBase(string eventName, string? callbackName)
    {
        EventName = eventName;
        CallbackName = callbackName;
    }
}