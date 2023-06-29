namespace Athena.Infrastructure.QueryFilters;

/// <summary>
/// 查询过滤器服务接口
/// </summary>
public interface IQueryFilterService
{
    /// <summary>
    /// 读取查询过滤器组
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<IList<QueryFilterGroup>?> GetAsync(string userId, Type type);
}