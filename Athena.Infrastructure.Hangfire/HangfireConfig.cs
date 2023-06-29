namespace Athena.Infrastructure.Hangfire;

/// <summary>
/// Hangfire Config
/// </summary>
public class HangfireConfig
{
    /// <summary>
    /// 队列
    /// </summary>
    public string[]? Queues { get; set; }

    /// <summary>
    /// Dashboard Config
    /// </summary>
    public HangfireDashboardInfo? Dashboard { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } =
        "127.0.0.1:6379,connectTimeout=30000,keepAlive=60,syncTimeout=5000,DefaultDatabase=0";
}

/// <summary>
/// Dashboard Config
/// </summary>
public class HangfireDashboardInfo
{
    /// <summary>
    /// PathMatch
    /// </summary>
    public string PathMatch { get; set; } = "/hangfire";

    /// <summary>
    /// Account
    /// </summary>
    public string Account { get; set; } = "admin";

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = "admin";
}