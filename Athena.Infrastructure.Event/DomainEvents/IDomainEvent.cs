namespace Athena.Infrastructure.Event.DomainEvents;

/// <summary>
/// 领域事件接口
/// </summary>
public interface IDomainEvent : INotification
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
}