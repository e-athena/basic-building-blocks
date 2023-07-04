namespace Athena.Infrastructure.Event.DomainEvents;

/// <summary>
/// 领域事件基类
/// <remarks>用于进程内通讯</remarks>
/// </summary>
public abstract class DomainEventBase : IDomainEvent
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
    /// 根追踪ID
    /// </summary>
    public string? RootTraceId { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    public string EventId { get; set; } = ObjectId.GenerateNewStringId();
}