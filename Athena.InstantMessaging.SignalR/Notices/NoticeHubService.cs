namespace Athena.InstantMessaging.SignalR.Notices;

/// <summary>
/// 
/// </summary>
public class NoticeHubService : INoticeHubService
{
    private readonly IHubContext<NoticeHub> _hubContext;
    private const string MethodName = "NoticeMessage";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hubContext"></param>
    public NoticeHubService(IHubContext<NoticeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// 给所有人发送通知消息
    /// </summary>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToAllAsync<TMessage>(TMessage message)
        where TMessage : class
    {
        await _hubContext.SendAllAsync(MethodName, message);
    }

    /// <summary>
    /// 给单个用户发送通知消息
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToUserAsync<TMessage>(string userId, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendUserAsync(MethodName, userId, message);
    }

    /// <summary>
    /// 给多个用户发送通知消息
    /// </summary>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToUsersAsync<TMessage>(IList<string> userIds, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendUsersAsync(MethodName, userIds, message);
    }

    /// <summary>
    /// 给单个用户组发送通知消息
    /// </summary>
    /// <param name="groupName">用户组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToGroupAsync<TMessage>(string groupName, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendGroupAsync(MethodName, groupName, message);
    }

    /// <summary>
    /// 给多个用户组发送通知消息
    /// </summary>
    /// <param name="groupNames">用户组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    public async Task SendMessageToGroupsAsync<TMessage>(IList<string> groupNames, TMessage message)
        where TMessage : class
    {
        await _hubContext.SendGroupsAsync(MethodName, groupNames, message);
    }
}