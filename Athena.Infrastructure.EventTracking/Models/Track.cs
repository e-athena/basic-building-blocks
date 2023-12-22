namespace Athena.Infrastructure.EventTracking.Models;

/// <summary>
/// 追踪信息
/// </summary>
[Table("event_tracking_tracks")]
[Index(nameof(TraceId))]
public class Track : EntityBase
{
    /// <summary>
    /// 业务Id
    /// </summary>
    public string? BusinessId { get; set; }

    /// <summary>
    /// 上级Id
    /// <remarks>为空时为根节点</remarks>
    /// </summary>
    [MaxLength(36)]
    public string? ParentId { get; set; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    [MaxLength(36)]
    public string TraceId { get; set; } = null!;

    /// <summary>
    /// 事件类型
    /// </summary>
    public EventType? EventType { get; set; }

    /// <summary>
    /// 事件名
    /// </summary>
    [MaxLength(64)]
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 事件类型全名
    /// </summary>
    [MaxLength(128)]
    public string? EventTypeFullName { get; set; }

    /// <summary>
    /// 执行状态
    /// <remarks>0、未执行，1、执行成功，2、执行失败</remarks>
    /// </summary>
    public TrackStatus TrackStatus { get; set; } = TrackStatus.NotExecute;

    /// <summary>
    /// 开始执行时间
    /// </summary>
    public DateTime? BeginExecuteTime { get; set; }

    /// <summary>
    /// 执行完成时间
    /// </summary>
    public DateTime? EndExecuteTime { get; set; }

    /// <summary>
    /// 事件参数
    /// </summary>
    [MaxLength(-1)]
    public string? Payload { get; set; }

    /// <summary>
    /// 处理器全名
    /// </summary>
    [MaxLength(128)]
    public string? ProcessorFullName { get; set; }

    /// <summary>
    /// 执行应用名
    /// </summary>
    public string ExecuteAppName { get; set; } = PlatformServices.Default.Application.ApplicationName;

    /// <summary>
    /// 异常信息
    /// </summary>
    [MaxLength(-1)]
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 异常内联信息
    /// </summary>
    [MaxLength(-1)]
    public string? ExceptionInnerMessage { get; set; }

    /// <summary>
    /// 异常内联类型
    /// </summary>
    public string? ExceptionInnerType { get; set; }

    /// <summary>
    /// 异常堆栈
    /// </summary>
    [MaxLength(-1)]
    public string? ExceptionStackTrace { get; set; }

    /// <summary>
    /// 执行开始
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="eventType"></param>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    /// <param name="processorType"></param>
    /// <param name="businessId"></param>
    /// <returns></returns>
    public static Track ExecuteBegin(
        string traceId,
        EventType? eventType,
        Type type, string payload, Type processorType, string? businessId = null)
    {
        return new Track
        {
            TraceId = traceId,
            EventType = eventType,
            EventTypeFullName = type.FullName,
            Payload = payload,
            ProcessorFullName = processorType.FullName,
            TrackStatus = TrackStatus.Executing,
            BeginExecuteTime = DateTime.Now,
            BusinessId = businessId
        };
    }

    /// <summary>
    /// 执行成功
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="eventType"></param>
    /// <param name="type"></param>
    /// <param name="processorType"></param>
    /// <returns></returns>
    public static Track ExecuteSuccess(
        string traceId,
        EventType? eventType,
        Type type, Type processorType)
    {
        return new Track
        {
            TraceId = traceId,
            EventType = eventType,
            EventTypeFullName = type.FullName,
            ProcessorFullName = processorType.FullName,
            TrackStatus = TrackStatus.Success,
            EndExecuteTime = DateTime.Now
        };
    }

    /// <summary>
    /// 执行失败
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="eventType"></param>
    /// <param name="type"></param>
    /// <param name="processorType"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static Track ExecuteFail(
        string traceId,
        EventType? eventType,
        Type type,
        Type processorType,
        Exception? exception
    )
    {
        return new Track
        {
            TraceId = traceId,
            EventType = eventType,
            EventTypeFullName = type.FullName,
            ProcessorFullName = processorType.FullName,
            ExceptionMessage = exception?.Message,
            ExceptionInnerMessage = exception?.InnerException?.Message,
            ExceptionInnerType = exception?.InnerException?.GetType().Name,
            ExceptionStackTrace = exception?.StackTrace,
            TrackStatus = TrackStatus.Fail,
            EndExecuteTime = DateTime.Now
        };
    }
}