using NewLife.Caching;

namespace Athena.Infrastructure.NewLifeRedis;

/// <summary>
/// RedisHelper
/// </summary>
public static class RedisHelper
{
    /// <summary>
    /// Redis实例
    /// </summary>
    public static FullRedis? Instance { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="cli"></param>
    public static void Initialization(FullRedis cli)
    {
        if (Instance != null)
        {
            throw new Exception("RedisHelper已经初始化");
        }

        Instance = cli;
    }
}