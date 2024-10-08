namespace Athena.Infrastructure.EventTracking.Models;

/// <summary>
/// 追踪信息
/// </summary>
[Table("event_tracking_tracks")]
[Index("trace_id")]
public class Track : EntityBase
{
    /// <summary>
    /// 业务Id
    /// </summary>
    [MaxLength(36)]
    [Column("business_id")]
    public string? BusinessId { get; set; }

    /// <summary>
    /// 上级Id
    /// <remarks>为空时为根节点</remarks>
    /// </summary>
    [MaxLength(36)]
    [Column("parent_id")]
    public string? ParentId { get; set; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    [MaxLength(36)]
    [Column("trace_id")]
    public string TraceId { get; set; } = null!;

    /// <summary>
    /// 事件类型
    /// </summary>
    [Column("event_type")]
    public EventType? EventType { get; set; }

    /// <summary>
    /// 事件名
    /// </summary>
    [MaxLength(64)]
    [Column("event_name")]
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 事件类型全名
    /// </summary>
    [MaxLength(128)]
    [Column("event_type_full_name")]
    public string? EventTypeFullName { get; set; }

    /// <summary>
    /// 执行状态
    /// <remarks>0、未执行，1、执行成功，2、执行失败</remarks>
    /// </summary>
    [Column("track_status")]
    public TrackStatus TrackStatus { get; set; } = TrackStatus.NotExecute;

    /// <summary>
    /// 开始执行时间
    /// </summary>
    [Column("begin_execute_time")]
    public DateTime? BeginExecuteTime { get; set; }

    /// <summary>
    /// 执行完成时间
    /// </summary>
    [Column("end_execute_time")]
    public DateTime? EndExecuteTime { get; set; }

    /// <summary>
    /// 事件参数
    /// </summary>
    [MaxLength(-1)]
    [Column("payload")]
    public string? Payload { get; set; }

    /// <summary>
    /// 处理器全名
    /// </summary>
    [MaxLength(128)]
    [Column("processor_full_name")]
    public string? ProcessorFullName { get; set; }

    /// <summary>
    /// 执行应用名
    /// </summary>
    [Column("execute_app_name")]
    public string ExecuteAppName { get; set; } = PlatformServices.Default.Application.ApplicationName;

    /// <summary>
    /// 异常信息
    /// </summary>
    [MaxLength(-1)]
    [Column("exception_message")]
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 异常内联信息
    /// </summary>
    [MaxLength(-1)]
    [Column("exception_inner_message")]
    public string? ExceptionInnerMessage { get; set; }

    /// <summary>
    /// 异常内联类型
    /// </summary>
    [Column("exception_inner_type")]
    public string? ExceptionInnerType { get; set; }

    /// <summary>
    /// 异常堆栈
    /// </summary>
    [MaxLength(-1)]
    [Column("exception_stack_trace")]
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