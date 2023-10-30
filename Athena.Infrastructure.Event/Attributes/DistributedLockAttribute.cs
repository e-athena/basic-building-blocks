using Athena.Infrastructure.DistributedLocks;
using Athena.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;
using Rougamo.Context;

namespace Athena.Infrastructure.Event.Attributes;

/// <summary>
/// 分布式锁
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DistributedLockAttribute : Rougamo.MoAttribute
{
    private const string ResourceName = "IntegratedEventHandler";
    private readonly IDistributedLock? _distributedLock = AthenaProvider.Provider?.GetService<IDistributedLock>();
    private ILockResource? _lockResource;

    /// <summary>
    /// 进入方法
    /// </summary>
    /// <param name="context"></param>
    public override void OnEntry(MethodContext context)
    {
        if (context.Arguments.Length == 0 || _distributedLock == null)
        {
            base.OnEntry(context);
            return;
        }

        if (context.Arguments[0] is not EventBase payload)
        {
            base.OnEntry(context);
            return;
        }

        var key = $"{context.TargetType.Name}_{payload.EventId}";
        _lockResource = _distributedLock
            .TryGetLockAsync(ResourceName, key)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (_lockResource == null)
        {
            throw new ResourceProcessedException("该资源有其他进程正在处理，请稍后再试");
        }

        base.OnEntry(context);
    }

    /// <summary>
    /// 退出方法
    /// </summary>
    /// <param name="context"></param>
    public override void OnExit(MethodContext context)
    {
        // 释放锁
        _lockResource?.ReleaseAsync();
        base.OnExit(context);
    }
}

/// <summary>
/// 资源被处理异常
/// </summary>
public class ResourceProcessedException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public ResourceProcessedException(string message) : base(message)
    {
    }
}