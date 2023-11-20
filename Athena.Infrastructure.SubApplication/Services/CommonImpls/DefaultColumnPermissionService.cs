using Athena.Infrastructure.ColumnPermissions;
using Athena.Infrastructure.ColumnPermissions.Models;
using Athena.Infrastructure.DataPermission.Attributes;

namespace Athena.Infrastructure.SubApplication.Services.CommonImpls;

/// <summary>
/// 列权限查询服务默认实现
/// </summary>
public class DefaultColumnPermissionService : IColumnPermissionService
{
    private readonly IUserService _userService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userService"></param>
    public DefaultColumnPermissionService(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<IList<ColumnPermission>?> GetAsync(string userId, Type type)
    {
        // 读取appId,如果没有则使用null
        // 检查type是否有DataPermissionAttribute，如果有，则从DataPermissionAttribute中读取AppId值
        // 如果没有，则使用null
        var info = type.GetCustomAttribute(typeof(DataPermissionAttribute), false) as DataPermissionAttribute;
        var appId = info?.AppId ?? null;
        var result = await _userService.GetUserColumnPermissionsAsync(appId, type.Name, userId);

        return result?.Select(p => new ColumnPermission
        {
            Enabled = p.Enabled,
            ColumnKey = p.ColumnKey,
            IsEnableDataMask = p.IsEnableDataMask,
            MaskLength = p.MaskLength,
            MaskPosition = p.MaskPosition,
            MaskChar = p.MaskChar,
        }).ToList();
    }
}