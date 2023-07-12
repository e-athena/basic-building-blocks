namespace Athena.Infrastructure.FreeRedis;

/// <summary>
/// RedisHelper
/// </summary>
public static class RedisHelper
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public static RedisClient? Instance { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="cli"></param>
    public static void Initialization(RedisClient cli)
    {
        if (Instance != null)
        {
            throw new Exception("RedisHelper已经初始化");
        }

        Instance = cli;
    }
}