namespace Athena.Infrastructure.Event.IntegrationEvents;

/// <summary>
/// 集成事件
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// 读取Id
    /// </summary>
    /// <returns></returns>
    string? GetId();

    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreatedOn { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    IDictionary<string, object> MetaData { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    string EventId { get; set; }

    /// <summary>
    /// 根跟踪Id
    /// </summary>
    string? RootTraceId { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    string EventName { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    string? TenantId { get; set; }
    
    /// <summary>
    /// 应用ID
    /// </summary>
    string? AppId { get; set; }

    /// <summary>
    /// CAP CallbackName
    /// </summary>
    string? CallbackName { get; set; }
}