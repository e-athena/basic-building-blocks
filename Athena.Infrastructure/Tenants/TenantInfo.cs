namespace Athena.Infrastructure.Tenants;

/// <summary>
/// 租户信息
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class TenantInfo
{
    /// <summary>
    /// DbKey
    /// </summary>
    public string DbKey { get; set; } = null!;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public int? DataType { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    public TenantIsolationLevel IsolationLevel { get; set; } = TenantIsolationLevel.Independent;
}

/// <summary>
/// 租户数据隔离方式
/// </summary>
public enum TenantIsolationLevel
{
    /// <summary>
    /// 独立数据库
    /// </summary>
    [Description("独立")] Independent = 1,

    /// <summary>
    /// 共享数据库
    /// </summary>
    [Description("共享")] Shared = 2
}
