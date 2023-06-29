namespace Athena.InstantMessaging;

/// <summary>
/// 事件Hub服务接口
/// </summary>
public interface IEventHubService
{
    /// <summary>
    /// 给所有人发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToAllAsync<TMessage>(string eventName, TMessage message) where TMessage : class;

    /// <summary>
    /// 给单个用户发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToUserAsync<TMessage>(string eventName, string userId, TMessage message) where TMessage : class;

    /// <summary>
    /// 给多个用户发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToUsersAsync<TMessage>(string eventName, IList<string> userIds, TMessage message)
        where TMessage : class;

    /// <summary>
    /// 给单个用户组发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="groupName">用户组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToGroupAsync<TMessage>(string eventName, string groupName, TMessage message)
        where TMessage : class;

    /// <summary>
    /// 给多个用户组发送事件消息
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="groupNames">用户组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToGroupsAsync<TMessage>(string eventName, IList<string> groupNames, TMessage message)
        where TMessage : class;
}