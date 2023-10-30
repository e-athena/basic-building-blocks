namespace Athena.Infrastructure.Caching;

/// <summary>
/// Redis配置
/// </summary>
public class RedisConfig
{
    /// <summary>
    /// 配置
    /// </summary>
    public string Configuration { get; set; } = "127.0.0.1:6379,connectTimeout=30000,keepAlive=60,syncTimeout=5000";

    /// <summary>
    /// 实例名称
    /// </summary>
    public string InstanceName { get; set; } = string.Empty;

    /// <summary>
    /// 默认数据库
    /// </summary>
    public int DefaultDatabase { get; set; }

    /// <summary>
    /// 哨兵列表
    /// </summary>
    public IList<string>? Sentinels { get; set; }
}