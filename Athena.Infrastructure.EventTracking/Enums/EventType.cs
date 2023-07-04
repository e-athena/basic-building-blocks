namespace Athena.Infrastructure.EventTracking.Enums;

/// <summary>
/// 事件类型
/// <remarks>1、领域事件，2、集成事件</remarks>
/// </summary>
public enum EventType
{
    /// <summary>
    /// 领域事件
    /// </summary>
    [Description("领域事件")] DomainEvent = 1,

    /// <summary>
    /// 集成事件
    /// </summary>
    [Description("集成事件")] IntegrationEvent = 2
}