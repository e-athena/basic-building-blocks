namespace Athena.Infrastructure.DataPermission;

/// <summary>
/// 数据权限服务接口
/// </summary>
public interface IDataPermissionService
{
    /// <summary>
    /// 读取用户已有的策略查询过滤器组列表
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="userId"></param>
    /// <param name="resourceKey"></param>
    /// <returns></returns>
    Task<List<QueryFilterGroup>> GetPolicyQueryFilterGroupsAsync(string userId, string resourceKey, string? appId);

    /// <summary>
    /// 获取用户所在组织/部门列表
    /// </summary>
    /// <param name="userId">关联的用户ID</param>
    /// <param name="appId"></param>
    /// <returns></returns>
    List<string> GetUserOrganizationIds(string userId, string? appId);

    /// <summary>
    /// 获取用户所在组织/部门列表
    /// </summary>
    /// <param name="userId">关联的用户ID</param>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<List<string>> GetUserOrganizationIdsAsync(string userId, string? appId);
    
    /// <summary>
    /// 获取用户所有组织/部门及下级组织/部门列表
    /// </summary>
    /// <param name="userId">关联的用户ID</param>
    /// <param name="appId"></param>
    /// <returns></returns>
    List<string> GetUserOrganizationIdsTree(string userId, string? appId);

    /// <summary>
    /// 获取用户所有组织/部门及下级组织/部门列表
    /// </summary>
    /// <param name="userId">关联的用户ID</param>
    /// <param name="appId"></param>
    /// <returns></returns>
    Task<List<string>> GetUserOrganizationIdsTreeAsync(string userId, string? appId);

    /// <summary>
    /// 读取用户角色的数据范围列表
    /// </summary>
    /// <returns></returns>
    List<Models.DataPermission> GetUserDataScopes(string userId, string? appId);

    /// <summary>
    /// 读取用户角色的数据范围列表
    /// </summary>
    /// <returns></returns>
    Task<List<Models.DataPermission>> GetUserDataScopesAsync(string userId, string? appId);
}