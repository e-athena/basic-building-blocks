namespace Athena.Infrastructure.EventStorage;

/// <summary>
/// 
/// </summary>
public class EventStorageOptions
{
    /// <summary>
    /// 启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 链接字符串
    /// </summary>
    public string ConnectionString { get; set; } = null!;
}