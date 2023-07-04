using System.Text.Json;
using Athena.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;
using Rougamo.Context;

namespace Athena.Infrastructure.EventTracking.Aop;

/// <summary>
/// 事件追踪
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EventTrackingAttribute : Rougamo.MoAttribute
{
    private readonly ITrackService? _trackService = AthenaProvider.Provider?.GetService<ITrackService>();

    // /// <summary>
    // /// 依赖事件类型
    // /// </summary>
    // public Type[]? DependentEventTypes { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnEntry(MethodContext context)
    {
        if (context.Arguments.Length == 0 || _trackService == null)
        {
            base.OnEntry(context);
            return;
        }

        var requestType = context.Arguments[0].GetType();
        var payloadStr = JsonSerializer.Serialize(context.Arguments[0]);
        var payload = JsonSerializer.Deserialize<EventTrackingBase>(payloadStr);
        if (payload?.RootTraceId != null)
        {
            _trackService.Write(
                Track.ExecuteBegin(
                    payload.RootTraceId,
                    payload.GetEventType(),
                    requestType,
                    payloadStr,
                    context.TargetType
                )
            );
        }

        base.OnEntry(context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnSuccess(MethodContext context)
    {
        if (context.Arguments.Length == 0 || _trackService == null)
        {
            base.OnSuccess(context);
            return;
        }

        var requestType = context.Arguments[0].GetType();
        var payloadStr = JsonSerializer.Serialize(context.Arguments[0]);
        var payload = JsonSerializer.Deserialize<EventTrackingBase>(payloadStr);
        if (payload?.RootTraceId != null)
        {
            _trackService.Write(
                Track.ExecuteSuccess(
                    payload.RootTraceId,
                    payload.GetEventType(),
                    requestType,
                    context.TargetType
                )
            );
        }

        base.OnSuccess(context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnException(MethodContext context)
    {
        if (context.Arguments.Length == 0 || _trackService == null)
        {
            base.OnSuccess(context);
            return;
        }

        var requestType = context.Arguments[0].GetType();
        var payloadStr = JsonSerializer.Serialize(context.Arguments[0]);
        var payload = JsonSerializer.Deserialize<EventTrackingBase>(payloadStr);
        if (payload?.RootTraceId != null)
        {
            _trackService.Write(
                Track.ExecuteFail(
                    payload.RootTraceId,
                    payload.GetEventType(),
                    requestType,
                    context.TargetType,
                    context.Exception
                )
            );
        }

        base.OnException(context);
    }
}

/// <summary>
/// 
/// </summary>
public class EventTrackingBase
{
    /// <summary>
    /// 根事件ID
    /// </summary>
    public string? RootTraceId { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    private IDictionary<string, object>? MetaData { get; set; }

    /// <summary>
    /// 读取事件类型
    /// </summary>
    /// <returns></returns>
    public EventType? GetEventType()
    {
        if (MetaData == null || MetaData.Count == 0)
        {
            return null;
        }

        if (!MetaData.TryGetValue("type", out var type))
        {
            return null;
        }

        return (EventType) Convert.ToInt32(type);
    }
}