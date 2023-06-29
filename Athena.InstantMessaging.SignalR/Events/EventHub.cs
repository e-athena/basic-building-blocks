namespace Athena.InstantMessaging.SignalR.Events;

/// <summary>
/// 事件Hub
/// </summary>
[Authorize]
public class EventHub : Hub
{
    private readonly ISecurityContextAccessor _securityContextAccessor;
    private readonly ILogger<EventHub> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="securityContextAccessor"></param>
    /// <param name="loggerFactory"></param>
    public EventHub(ISecurityContextAccessor securityContextAccessor, ILoggerFactory loggerFactory)
    {
        _securityContextAccessor = securityContextAccessor;
        _logger = loggerFactory.CreateLogger<EventHub>();
    }

    /// <summary>
    /// 用户连接成功后
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        // 每个角色一个组
        var roles = _securityContextAccessor.Roles ?? new List<string>();
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }

        if (roles.Count > 0)
        {
            _logger.LogTrace(
                "{Now} - [{RealName}({UserName})]已上线，已加入[{RoleName}]群组，连接ID:{ConnectionId}，IP地址:{IP}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _securityContextAccessor.RealName,
                _securityContextAccessor.UserName,
                _securityContextAccessor.RoleName,
                Context.ConnectionId,
                _securityContextAccessor.IpAddress
            );
        }
        else
        {
            _logger.LogTrace("{Now} - [{RealName}]已上线，连接ID:{ConnectionId}，IP地址:{IP}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _securityContextAccessor.RealName,
                Context.ConnectionId,
                _securityContextAccessor.IpAddress
            );
        }

        await Clients.Client(Context.ConnectionId)
            .SendAsync("system", $"成功连接至服务器，当前服务器时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    /// <summary>
    /// 用户断开连接后
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // 每个角色一个组
        var roles = _securityContextAccessor.Roles ?? new List<string>();
        foreach (var role in roles)
        {
            Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
        }

        if (roles.Count > 0)
        {
            _logger.LogTrace(
                "{Now} - [{RealName}({UserName})]已下线，已退出[{RoleName}]群组，连接ID:{ConnectionId}，IP地址:{IP}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _securityContextAccessor.RealName,
                _securityContextAccessor.UserName,
                _securityContextAccessor.RoleName,
                Context.ConnectionId,
                _securityContextAccessor.IpAddress
            );
        }
        else
        {
            _logger.LogTrace("{Now} - [{RealName}]已下线，连接ID:{ConnectionId}，IP地址:{IP}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _securityContextAccessor.RealName,
                Context.ConnectionId,
                _securityContextAccessor.IpAddress
            );
        }

        return base.OnDisconnectedAsync(exception);
    }
}