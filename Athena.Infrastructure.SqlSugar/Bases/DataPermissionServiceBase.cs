namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 数据权限服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataPermissionServiceBase<T> : ServiceBase<T> where T : FullEntityCore, new()
{
    private readonly IDataPermissionService? _dataPermissionService;
    private readonly ISecurityContextAccessor _accessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="accessor"></param>
    public DataPermissionServiceBase(ISqlSugarClient sqlSugarClient, ISecurityContextAccessor accessor) :
        base(sqlSugarClient, accessor)
    {
        _accessor = accessor;
        _dataPermissionService =
            AthenaProvider.Provider?.GetService(typeof(IDataPermissionService)) as IDataPermissionService;
    }

    /// <summary>
    /// 创建人组织架构Ids
    /// </summary>
    protected override string? OrganizationalUnitIds
    {
        get
        {
            if (IsRoot || IsTenantAdmin)
            {
                return null;
            }

            var orgList = GetUserOrganizationIds();
            return orgList.Count == 0 ? null : string.Join(",", orgList);
        }
    }

    /// <summary>
    /// 读取用户组织架构ID列表
    /// </summary>
    /// <returns></returns>
    protected List<string> GetUserOrganizationIds(string? userId = null)
    {
        userId ??= UserId;

        if (_dataPermissionService == null || userId == null)
        {
            return new List<string>();
        }

        return _dataPermissionService.GetUserOrganizationIds(userId, _accessor.AppId);
    }
}