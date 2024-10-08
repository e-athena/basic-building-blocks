using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Athena.Infrastructure.DataAnnotations.Schema;
using Athena.Infrastructure.Event;
using Athena.Infrastructure.Event.DomainEvents;
using Athena.Infrastructure.Event.IntegrationEvents;

namespace Athena.Infrastructure.Domain;

/// <summary>
/// Entity基类
/// </summary>
[Index("created_on", IsUnique = false)]
[Index("updated_on", IsUnique = false)]
public abstract class EntityCore : IAggregateRoot
{
    /// <summary>
    /// ID
    /// </summary>
    [Required]
    [MaxLength(36)]
    [Key]
    [Column("id")]
    public string Id { get; set; } = ObjectId.GenerateNewStringId();

    /// <summary>
    /// 版本号
    /// </summary>
    [Required]
    [RowVersion]
    [FieldSort(999)]
    [Column("version")]
    public long Version { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    [FieldSort(999)]
    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    /// <summary>
    /// 更新时间
    /// </summary>
    [FieldSort(999)]
    [Column("updated_on")]
    public DateTime? UpdatedOn { get; set; } = DateTime.Now;

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
        if (!string.IsNullOrEmpty(id))
        {
            Id = id;
        }
    }

    /// <summary>
    /// 添加领域事件
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

/// <summary>
/// 组织架构数据权限
/// </summary>
[Table("business_org_auths")]
[Index("organizational_unit_id", IsUnique = false)]
[Index("business_table", "business_id", IsUnique = true)]
public class OrganizationalUnitAuth : ValueObject
{
    /// <summary>
    /// 组织ID
    /// </summary>
    [MaxLength(36)]
    [Column("organizational_unit_id")]
    public string OrganizationalUnitId { get; set; } = null!;

    /// <summary>
    /// 业务ID
    /// </summary>
    [MaxLength(36)]
    [Column("business_id")]
    public string BusinessId { get; set; } = null!;

    /// <summary>
    /// 业务表
    /// </summary>
    [MaxLength(64)]
    [Column("business_table")]
    public string BusinessTable { get; set; } = null!;

    /// <summary>
    ///
    /// </summary>
    public OrganizationalUnitAuth()
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="organizationalUnitId"></param>
    /// <param name="businessId"></param>
    /// <param name="businessTable"></param>
    public OrganizationalUnitAuth(string organizationalUnitId, string businessId, string businessTable)
    {
        OrganizationalUnitId = organizationalUnitId;
        BusinessId = businessId;
        BusinessTable = businessTable;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="id"></param>
    /// <param name="organizationalUnitId"></param>
    /// <param name="businessId"></param>
    /// <param name="businessTable"></param>
    public OrganizationalUnitAuth(long id, string organizationalUnitId, string businessId, string businessTable) :
        base(id)
    {
        OrganizationalUnitId = organizationalUnitId;
        BusinessId = businessId;
        BusinessTable = businessTable;
    }
}