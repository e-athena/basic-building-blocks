using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Athena.Infrastructure.Domain.Attributes;
using Athena.Infrastructure.Event;
using Athena.Infrastructure.Event.DomainEvents;
using Athena.Infrastructure.Event.IntegrationEvents;

namespace Athena.Infrastructure.Domain;

/// <summary>
/// Entity基类
/// </summary>
public abstract class EntityCore : IAggregateRoot
{
    /// <summary>
    /// ID
    /// </summary>
    [Required]
    [MaxLength(36)]
    [Key]
    public string Id { get; set; } = ObjectId.GenerateNewStringId();

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedOn { get; set; } = DateTime.Now;

    /// <summary>
    /// 版本号
    /// </summary>
    [Required]
    [RowVersion]
    public long Version { get; set; }

    /// <summary>
    /// 领域事件
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public HashSet<IDomainEvent> DomainEvents { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public EntityCore()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public EntityCore(string id)
    {
        Id = id;
    }

    /// <summary>
    /// 添加领域事件[兼容ENode的方法]
    /// </summary>
    /// <param name="eventItem"></param>
    public void ApplyEvent(EventBase eventItem)
    {
        DomainEvents.Add(eventItem);
        IntegrationEvents.Add(eventItem);
    }

    /// <summary>
    /// 添加领域事件
    /// </summary>
    /// <param name="eventItem"></param>
    public void AddDomainEvent(DomainEventBase eventItem)
    {
        DomainEvents.Add(eventItem);
    }

    /// <summary>
    /// 移除领域事件
    /// </summary>
    /// <param name="eventItem"></param>
    public void RemoveDomainEvent(DomainEventBase eventItem)
    {
        DomainEvents.Remove(eventItem);
    }

    /// <summary>
    /// 集成事件
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public HashSet<IIntegrationEvent> IntegrationEvents { get; set; } = new();

    /// <summary>
    /// 添加集成事件
    /// </summary>
    /// <param name="eventItem"></param>
    public void AddIntegrationEvent(IntegrationEventBase eventItem)
    {
        IntegrationEvents.Add(eventItem);
    }

    /// <summary>
    /// 移除集成事件
    /// </summary>
    /// <param name="eventItem"></param>
    public void RemoveIntegrationEvent(IntegrationEventBase eventItem)
    {
        IntegrationEvents.Remove(eventItem);
    }
}