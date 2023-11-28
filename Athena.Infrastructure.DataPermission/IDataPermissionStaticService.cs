namespace Athena.Infrastructure.DataPermission;

/// <summary>
/// 数据权限静态服务接口
/// </summary>
public interface IDataPermissionStaticService
{
    /// <summary>
    /// 读取数据权限配置列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="assemblyKeyword">程序集关键字</param>
    /// <returns></returns>
    IEnumerable<Models.DataPermission> GetList(string appId, string? assemblyKeyword = null);

    /// <summary>
    /// 读取数据权限配置列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="assemblyKeywords">程序集关键字</param>
    /// <returns></returns>
    IEnumerable<Models.DataPermission> GetList(string appId, params string[] assemblyKeywords);

    /// <summary>
    /// 读取数据权限配置列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="permissions"></param>
    /// <param name="assemblyKeyword">程序集关键字</param>
    /// <returns></returns>
    IEnumerable<Models.DataPermission> GetList(
        string appId,
        IList<Models.DataPermission>? permissions,
        string? assemblyKeyword = null
    );

    /// <summary>
    /// 读取数据权限配置列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="permissions"></param>
    /// <param name="assemblyKeywords">程序集关键字</param>
    /// <returns></returns>
    IEnumerable<Models.DataPermission> GetList(
        string appId,
        IList<Models.DataPermission>? permissions,
        params string[] assemblyKeywords
    );

    /// <summary>
    /// 读取数据权限配置树列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="permissions">拥有的权限</param>
    /// <param name="assemblyKeyword">程序集关键字</param>
    /// <returns></returns>
    IList<DataPermissionGroup> GetGroupList(
        string appId,
        IList<Models.DataPermission>? permissions = null,
        string? assemblyKeyword = null
    );

    /// <summary>
    /// 读取数据权限配置树列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="permissions">拥有的权限</param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    IList<DataPermissionGroup> GetGroupList(
        string appId,
        IList<Models.DataPermission>? permissions = null,
        params string[] assemblyKeywords
    );

    /// <summary>
    /// 获取树形结构列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="assemblyKeyword">程序集关键字</param>
    /// <returns></returns>
    List<DataPermissionTree> GetTreeList(string appId,
        string? assemblyKeyword = null
    );

    /// <summary>
    /// 获取树形结构列表
    /// </summary>
    /// <param name="appId">应用ID</param>
    /// <param name="assemblyKeywords">程序集关键字</param>
    /// <returns></returns>
    List<DataPermissionTree> GetTreeList(
        string appId,
        params string[] assemblyKeywords
    );
}