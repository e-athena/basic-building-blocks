namespace Athena.InstantMessaging.SignalR.Events;

/// <summary>
/// 
/// </summary>
public class EventHubService : IEventHubService
{
    private readonly IHubContext<EventHub> _hubContext;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hubContext"></param>
    public EventHubService(IHubContext<EventHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// 给所有人发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToAllAsync<TMessage>(string eventName, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendAllAsync(eventName, message);
    }

    /// <summary>
    /// 给单个用户发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToUserAsync<TMessage>(string eventName, string userId, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendUserAsync(eventName, userId, message);
    }

    /// <summary>
    /// 给多个用户发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToUsersAsync<TMessage>(string eventName, IList<string> userIds, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendUsersAsync(eventName, userIds, message);
    }

    /// <summary>
    /// 给单个用户组发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="groupName">用户组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToGroupAsync<TMessage>(string eventName, string groupName, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendGroupAsync(eventName, groupName, message);
    }

    /// <summary>
    /// 给多个用户组发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="groupNames">用户组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToGroupsAsync<TMessage>(string eventName, IList<string> groupNames,
        TMessage message)
        where TMessage : class
    {
        await _hubContext.SendGroupsAsync(eventName, groupNames, message);
    }
}