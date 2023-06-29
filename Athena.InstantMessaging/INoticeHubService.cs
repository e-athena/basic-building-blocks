namespace Athena.InstantMessaging;

/// <summary>
/// 通知Hub服务接口
/// </summary>
public interface INoticeHubService
{
    /// <summary>
    /// 给所有人发送通知消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToAllAsync<TMessage>(TMessage message) where TMessage : class;

    /// <summary>
    /// 给单个用户发送通知消息
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToUserAsync<TMessage>(string userId, TMessage message) where TMessage : class;

    /// <summary>
    /// 给多个用户发送通知消息
    /// </summary>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToUsersAsync<TMessage>(IList<string> userIds, TMessage message) where TMessage : class;

    /// <summary>
    /// 给单个用户组发送通知消息
    /// </summary>
    /// <param name="groupName">用户组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToGroupAsync<TMessage>(string groupName, TMessage message) where TMessage : class;

    /// <summary>
    /// 给多个用户组发送通知消息
    /// </summary>
    /// <param name="groupNames">用户组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    Task SendMessageToGroupsAsync<TMessage>(IList<string> groupNames, TMessage message) where TMessage : class;
}