using Rougamo.Context;

namespace Athena.Infrastructure.SubApplication.Aop;

/// <summary>
/// 服务调用异常日志
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ServiceInvokeExceptionLoggingAttribute : Rougamo.ExMoAttribute
{
    private readonly IOptionsMonitor<ServiceCallConfig>? _options =
        AthenaProvider.Provider?.GetService<IOptionsMonitor<ServiceCallConfig>>();

    private readonly ILoggerFactory? _loggerFactory = AthenaProvider.Provider?.GetService<ILoggerFactory>();


    /// <summary>
    /// 异常处理
    /// </summary>
    /// <param name="context"></param>
    protected override void ExOnException(MethodContext context)
    {
        if (_options == null || _options.CurrentValue.ThrowException)
        {
            base.ExOnException(context);
            return;
        }

        _loggerFactory?
            .CreateLogger<ServiceInvokeExceptionLoggingAttribute>()
            .LogError(context.Exception, "服务调用异常，异常信息:{Message}", context.Exception?.Message);

        var returnValue = context.ExReturnType == null ? null : Activator.CreateInstance(context.ExReturnType);
        context.HandledException(this, returnValue!);

        base.ExOnException(context);
    }
}