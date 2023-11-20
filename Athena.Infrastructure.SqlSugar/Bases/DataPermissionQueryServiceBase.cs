namespace Athena.Infrastructure.SqlSugar.Bases;

/// <summary>
/// 数据权限查询服务基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataPermissionQueryServiceBase<T> : QueryServiceBase<T> where T : FullEntityCore, new()
{
    private readonly IDataPermissionService? _dataPermissionService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    /// <param name="accessor"></param>
    public DataPermissionQueryServiceBase(
        ISqlSugarClient sqlSugarClient,
        ISecurityContextAccessor accessor
    ) : base(sqlSugarClient, accessor)
    {
        _dataPermissionService =
            AthenaProvider.Provider?.GetService(typeof(IDataPermissionService)) as IDataPermissionService;
    }

    /// <summary>
    /// 跳过权限查询
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    protected ISugarQueryable<T1> QuerySkipPermission<T1>() where T1 : class, new()
    {
        return DbContext.Queryable<T1>();
    }

    /// <summary>
    /// 跳过权限查询
    /// </summary>
    protected ISugarQueryable<T> QueryableSkipPermission => QuerySkipPermission<T>();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected override ISugarQueryable<T> Queryable => QueryWithPermission<T>();

    /// <summary>
    /// 查询对象
    /// </summary>
    protected override ISugarQueryable<T> QueryableNoTracking => QueryNoTrackingWithPermission<T>();

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <returns></returns>
    protected override ISugarQueryable<T> Query()
    {
        return QueryWithPermission<T>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <returns></returns>
    protected override ISugarQueryable<T> QueryNoTracking()
    {
        return QueryNoTrackingWithPermission<T>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    protected override ISugarQueryable<T1> Query<T1>()
    {
        return QueryWithPermission<T1>();
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    protected override ISugarQueryable<T1> QueryNoTracking<T1>()
    {
        return QueryNoTrackingWithPermission<T1>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    private ISugarQueryable<T1> QueryWithPermission<T1>() where T1 : class, new()
    {
        var query = DbContext.Queryable<T1>();
        return QueryWithPermission(query);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    private ISugarQueryable<T1> QueryNoTrackingWithPermission<T1>() where T1 : class, new()
    {
        var query = DbContext.Queryable<T1>();
        return QueryWithPermission(query);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    private ISugarQueryable<T1> QueryWithPermission<T1>(ISugarQueryable<T1> query) where T1 : class, new()
    {
        // 如果是开发者帐号。则不需要过滤
        if (IsRoot || IsTenantAdmin)
        {
            return query;
        }

        // 数据访问范围
        var dataScopeList = GetUserDataScopes();
        // 读取当前模块的数据访问范围
        var dataScope = dataScopeList
            .FirstOrDefault(p => typeof(T1).Name == p.ResourceKey);
        // 如果该模块有全部数据的权限则不需要过滤
        if (dataScope == null)
        {
            var emptyResourceKeyDataPermissions = dataScopeList
                .Where(p => string.IsNullOrEmpty(p.ResourceKey))
                .Select(p => new Athena.Infrastructure.DataPermission.Models.DataPermission
                {
                    DataScope = p.DataScope,
                    DataScopeCustom = p.DataScopeCustom
                })
                .ToList();

            // 查询通用设置，如果包含全部的数据。则不需要过滤
            if (emptyResourceKeyDataPermissions.Any(dp => dp.DataScope == RoleDataScope.All))
            {
                return query;
            }

            var filterWhere1 = GenerateFilterWhere<T1>(emptyResourceKeyDataPermissions);
            // 如果没有任何数据权限，则返回空
            return query.Where(filterWhere1 ?? (p => false));
        }

        // 当前模块有全部数据的权限则不需要过滤
        if (dataScope.DataScope == RoleDataScope.All)
        {
            return query;
        }

        var dataPermissions = new List<Athena.Infrastructure.DataPermission.Models.DataPermission>
        {
            dataScope
        };
        var filterWhere = GenerateFilterWhere<T1>(dataPermissions);
        // 如果没有任何数据权限，则返回空
        return query.Where(filterWhere ?? (p => false));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <typeparam name="T1"></typeparam>
    /// <returns></returns>
    private static bool HasProperty<T1>(string propertyName)
    {
        return typeof(T1).GetProperties().Any(p => p.Name == propertyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataPermissions"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    private Expression<Func<TResult, bool>>? GenerateFilterWhere<TResult>(
        ICollection<Athena.Infrastructure.DataPermission.Models.DataPermission> dataPermissions)
        where TResult : class, new()
    {
        if (dataPermissions.Count == 0)
        {
            return null;
        }

        var filters = new List<QueryFilter>();
        var organizationIds = new List<string>();
        foreach (var data in dataPermissions)
        {
            if (data.DataScope == RoleDataScope.Self)
            {
                if (!HasProperty<TResult>("CreatedUserId"))
                {
                    continue;
                }

                filters.Add(new QueryFilter
                {
                    Key = "CreatedUserId",
                    Operator = "==",
                    Value = UserId!,
                    XOR = "or"
                });
                continue;
            }

            if (!HasProperty<TResult>("OrganizationalUnitIds"))
            {
                continue;
            }

            var orgIds = data.DataScope switch
            {
                RoleDataScope.Department => GetUserOrganizationIds(),
                RoleDataScope.DepartmentAndSub => GetUserOrganizationIdsTree(),
                RoleDataScope.Custom => data.DataScopeCustoms,
                _ => null
            };

            if (orgIds == null || orgIds.Count == 0)
            {
                continue;
            }

            organizationIds.AddRange(orgIds);
        }

        if (organizationIds.Count <= 0)
        {
            return QueryableExtensions.MakeFilterWhere<TResult>(filters);
        }

        // 去重
        organizationIds = organizationIds.GroupBy(p => p).Select(p => p.Key).ToList();

        filters.Add(new QueryFilter
        {
            Key = "OrganizationalUnitIds",
            Operator = "in",
            Value = string.Join(",", organizationIds),
            XOR = "or"
        });

        // 生成sql
        var businessSql0 = DbContext.Queryable<OrganizationalUnitAuth>()
            .AS("business_org_auths")
            .Where(p => organizationIds.Contains(p.OrganizationalUnitId))
            .Where(p => p.BusinessTable == typeof(TResult).Name)
            .Select(p => p.BusinessId)
            .ToSqlString();

        filters.Add(new QueryFilter
        {
            Key = "Id",
            Operator = "sub_query",
            Value = businessSql0,
            XOR = "or"
        });

        // foreach (var orgId in organizationIds)
        // {
        //     filters.Add(new QueryFilter
        //     {
        //         Key = "OrganizationalUnitIds",
        //         Operator = "contains",
        //         Value = orgId,
        //         XOR = "or"
        //     });
        // }

        return QueryableExtensions.MakeFilterWhere<TResult>(filters);
    }

    #region 数据查询权限相关

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

        return _dataPermissionService.GetUserOrganizationIds(userId, null);
    }

    /// <summary>
    /// 读取用户组织架构及下级组织架构ID列表
    /// </summary>
    /// <returns></returns>
    protected List<string> GetUserOrganizationIdsTree(string? userId = null)
    {
        userId ??= UserId;

        if (_dataPermissionService == null || userId == null)
        {
            return new List<string>();
        }

        return _dataPermissionService.GetUserOrganizationIdsTree(userId, null);
    }

    /// <summary>
    /// 读取用户角色的数据范围列表
    /// </summary>
    /// <returns></returns>
    private List<Athena.Infrastructure.DataPermission.Models.DataPermission> GetUserDataScopes(string? userId = null)
    {
        userId ??= UserId;

        if (_dataPermissionService == null || userId == null)
        {
            return new List<Athena.Infrastructure.DataPermission.Models.DataPermission>();
        }

        return _dataPermissionService.GetUserDataScopes(userId, null);
    }

    #endregion
}