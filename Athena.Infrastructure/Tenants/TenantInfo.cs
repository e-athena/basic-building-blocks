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
}