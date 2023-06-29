namespace Athena.Infrastructure.FreeSql.Tenants;

/// <summary>
/// 多租户的FreeSql
/// </summary>
public class FreeSqlMultiTenancy : FreeSqlCloud<string>
{
    /// <summary>
    /// 
    /// </summary>
    public FreeSqlMultiTenancy() : base(null)
    {
    }
}