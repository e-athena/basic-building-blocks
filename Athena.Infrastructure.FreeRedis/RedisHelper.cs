namespace Athena.Infrastructure.FreeRedis;

/// <summary>
/// RedisHelper
/// </summary>
public static class RedisHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static RedisClient? Instance { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="cli"></param>
    public static void Initialization(RedisClient cli)
    {
        Instance = cli;
    }
}