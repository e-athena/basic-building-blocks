using System.Text.Json;
using Athena.Infrastructure.Event;
using Athena.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rougamo.Context;

namespace Athena.Infrastructure.EventTracking.Aop;

/// <summary>
/// 事件追踪
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EventTrackingAttribute : Rougamo.MoAttribute
{
    private readonly ITrackService? _trackService = AthenaProvider.Provider?.GetService<ITrackService>();
    
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

        if (context.Arguments[0] is not EventBase payload)
        {
            base.OnEntry(context);
            return;
        }

        try
        {
            var requestType = payload.GetType();
            var payloadStr = JsonSerializer.Serialize(context.Arguments[0]);
            if (payload.RootTraceId != null)
            {
                _trackService.Write(
                    Track.ExecuteBegin(
                        payload.RootTraceId,
                        GetEventType(payload.MetaData),
                        requestType,
                        payloadStr,
                        context.TargetType,
                        GetBusinessId(payload.MetaData)
                    )
                );
            }
        }
        catch (Exception exception)
        {
            // ignored
            AthenaProvider.DefaultLog?.LogError(exception, "EventTrackingAttribute.OnEntry");
        }
        finally
        {
            base.OnEntry(context);
        }
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

        if (context.Arguments[0] is not EventBase payload)
        {
            base.OnSuccess(context);
            return;
        }

        try
        {
            var requestType = payload.GetType();
            if (payload.RootTraceId != null)
            {
                _trackService.Write(
                    Track.ExecuteSuccess(
                        payload.RootTraceId,
                        GetEventType(payload.MetaData),
                        requestType,
                        context.TargetType
                    )
                );
            }
        }
        catch (Exception exception)
        {
            // ignored
            AthenaProvider.DefaultLog?.LogError(exception, "EventTrackingAttribute.OnSuccess");
        }
        finally
        {
            base.OnSuccess(context);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnException(MethodContext context)
    {
        if (context.Arguments.Length == 0 || _trackService == null)
        {
            base.OnException(context);
            return;
        }

        if (context.Arguments[0] is not EventBase payload)
        {
            base.OnException(context);
            return;
        }

        try
        {
            var requestType = payload.GetType();
            if (payload.RootTraceId != null)
            {
                _trackService.Write(
                    Track.ExecuteFail(
                        payload.RootTraceId,
                        GetEventType(payload.MetaData),
                        requestType,
                        context.TargetType,
                        context.Exception
                    )
                );
            }
        }
        catch (Exception exception)
        {
            // ignored
            AthenaProvider.DefaultLog?.LogError(exception, "EventTrackingAttribute.OnException");
        }
        finally
        {
            base.OnException(context);
        }
    }

    /// <summary>
    /// 读取事件类型
    /// </summary>
    /// <returns></returns>
    private static EventType? GetEventType(IDictionary<string, object> metaData)
    {
        if (metaData.Count == 0)
        {
            return null;
        }

        if (!metaData.TryGetValue("type", out var type))
        {
            return null;
        }

        if (int.TryParse(type.ToString(), out var intType))
        {
            return (EventType) intType;
        }

        return null;
    }

    /// <summary>
    /// 读取业务Id
    /// </summary>
    /// <returns></returns>
    private static string? GetBusinessId(IDictionary<string, object> metaData)
    {
        if (metaData.Count == 0)
        {
            return null;
        }

        return !metaData.TryGetValue("id", out var id) ? null : id.ToString();
    }
}