namespace Athena.Infrastructure.FreeSql.Tenants;

/// <summary>
/// 多租户
/// </summary>
public static class FreeSqlMultiTenancyManager
{
    /// <summary>
    /// 多租户
    /// </summary>
    public static readonly FreeSqlMultiTenancy Instance = new();
}