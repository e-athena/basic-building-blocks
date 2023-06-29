namespace Athena.Infrastructure.Event.Interfaces;

/// <summary>
/// 集成事件标记，实现该接口的会被自动注册
/// </summary>
public interface IIntegratedEventMarker : ICapSubscribe
{
}