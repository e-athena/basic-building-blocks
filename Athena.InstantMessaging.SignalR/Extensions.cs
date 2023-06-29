using Athena.InstantMessaging;
using Athena.InstantMessaging.SignalR;
using Athena.InstantMessaging.SignalR.Events;
using Athena.InstantMessaging.SignalR.Notices;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 扩展类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="actionHubOptions"></param>
    /// <returns></returns>
    private static ISignalRServerBuilder AddCustomSignalR(
        this IServiceCollection services,
        Action<HubOptions>? actionHubOptions = null)
    {
        return services.AddSignalR(options => { actionHubOptions?.Invoke(options); });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="actionHubOptions"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    public static ISignalRServerBuilder AddCustomSignalRWithRedis(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HubOptions>? actionHubOptions = null,
        Action<RedisOptions>? setupAction = null)
    {
        // 注入服务
        services.AddSingleton<INoticeHubService, NoticeHubService>();
        services.AddSingleton<IEventHubService, EventHubService>();

        return services
            .AddCustomSignalR(options => { actionHubOptions?.Invoke(options); })
            .AddStackExchangeRedis(options =>
            {
                var configValue = configuration
                    .GetConfigValue("SignalRRedisConfiguration", "SIGNALR_REDIS_CONFIGURATION", true);
                // 不为null
                if (!string.IsNullOrEmpty(configValue))
                {
                    options.Configuration = ConfigurationOptions.Parse(configValue);
                    setupAction?.Invoke(options);
                    return;
                }

                var config = configuration.GetRedisConfig();
                var configurationOptions = ConfigurationOptions.Parse(config.Configuration);
                configurationOptions.DefaultDatabase = config.DefaultDatabase;
                configurationOptions.ChannelPrefix = config.InstanceName + "signalr:";
                options.Configuration = configurationOptions;

                setupAction?.Invoke(options);
            });
    }

    #region Basic

    /// <summary>
    /// 给所有在线用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method">方法</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendAllAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string method,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.Clients.All.SendAsync(method, message);
    }

    /// <summary>
    /// 给一个用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method">方法</param>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUserAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string method,
        string userId,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.Clients.User(userId).SendAsync(method, message);
    }

    /// <summary>
    /// 给多个用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method">方法</param>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUsersAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string method,
        IEnumerable<string> userIds,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.Clients.Users(userIds).SendAsync(method, message);
    }

    /// <summary>
    /// 给一个组发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method">方法</param>
    /// <param name="groupName">组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string method,
        string groupName,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.Clients.Group(groupName).SendAsync(method, message);
    }

    /// <summary>
    /// 给多个组发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="method">方法</param>
    /// <param name="groupNames">组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupsAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string method,
        IEnumerable<string> groupNames,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.Clients.Groups(groupNames).SendAsync(method, message);
    }

    #endregion

    #region Message

    /// <summary>
    /// 给所有用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendAllMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendAllAsync("Message", message);
    }

    /// <summary>
    /// 给一个用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUserMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string userId,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendUserAsync("Message", userId, message);
    }

    /// <summary>
    /// 给多个用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUsersMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        IEnumerable<string> userIds,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendUsersAsync("Message", userIds, message);
    }

    /// <summary>
    /// 给一个组发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groupName">组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string groupName,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendGroupAsync("Message", groupName, message);
    }

    /// <summary>
    /// 给多个组发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groupNames">组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupsMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        IEnumerable<string> groupNames,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendGroupsAsync("Message", groupNames, message);
    }

    #endregion

    #region SystemMessage

    /// <summary>
    /// 给所有用户发送系统消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendAllSystemMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendAllAsync("SystemMessage", message);
    }

    /// <summary>
    /// 给一个用户发送消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUserSystemMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string userId,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendUserAsync("SystemMessage", userId, message);
    }

    /// <summary>
    /// 给多个用户发送系统消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUsersSystemMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        IEnumerable<string> userIds,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendUsersAsync("SystemMessage", userIds, message);
    }

    /// <summary>
    /// 给一个组发送系统消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groupName">组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupSystemMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string groupName,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendGroupAsync("SystemMessage", groupName, message);
    }

    /// <summary>
    /// 给多个组发送系统消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groupNames">组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupsSystemMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        IEnumerable<string> groupNames,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendGroupsAsync("SystemMessage", groupNames, message);
    }

    #endregion

    #region ReceiveMessage

    /// <summary>
    /// 给所有用户发送系统消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendAllReceiveMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendAllAsync("ReceiveMessage", message);
    }

    /// <summary>
    /// 给一个用户发送回复消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userId">用户Id</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUserReceiveMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string userId,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendUserAsync("ReceiveMessage", userId, message);
    }

    /// <summary>
    /// 给多个用户发送回复消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userIds">用户Id列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendUsersReceiveMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        IEnumerable<string> userIds,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendUsersAsync("ReceiveMessage", userIds, message);
    }

    /// <summary>
    /// 给一个组发送回复消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groupName">组名</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupReceiveMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        string groupName,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendGroupAsync("ReceiveMessage", groupName, message);
    }

    /// <summary>
    /// 给多个组发送回复消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="groupNames">组名列表</param>
    /// <param name="message">消息内容</param>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public static async Task SendGroupsReceiveMessageAsync<THub, TMessage>(
        this IHubContext<THub> context,
        IEnumerable<string> groupNames,
        TMessage message
    ) where THub : Hub where TMessage : class
    {
        await context.SendGroupsAsync("ReceiveMessage", groupNames, message);
    }

    #endregion

    /// <summary>
    /// 读取Redis配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    private static RedisConfig GetRedisConfig(
        this IConfiguration configuration,
        string configVariable = "RedisConfig",
        string envVariable = "REDIS_CONFIG")
    {
        return configuration.GetConfig<RedisConfig>(configVariable, envVariable);
    }
}