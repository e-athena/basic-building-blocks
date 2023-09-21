using Athena.Infrastructure.ColumnPermissions.Models;

namespace Athena.Infrastructure.ColumnPermissions;

/// <summary>
/// 列权限查询服务接口
/// </summary>
public interface IColumnPermissionService
{
    /// <summary>
    /// 读取列权限列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<IList<ColumnPermission>?> GetAsync(string userId, Type type);
}