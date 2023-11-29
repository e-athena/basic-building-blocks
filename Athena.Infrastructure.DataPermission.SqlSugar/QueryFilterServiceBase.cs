namespace Athena.Infrastructure.DataPermission.SqlSugar;

/// <summary>
/// 查询过滤器服务基类
/// </summary>
public class QueryFilterServiceBase
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly DataPermissionFactory _dataPermissionFactory;
    private readonly IDataPermissionService _dataPermissionService;

    public QueryFilterServiceBase(
        ISqlSugarClient sqlSugarClient,
        IEnumerable<IDataPermission> dataPermissions,
        IDataPermissionService dataPermissionService
    )
    {
        _sqlSugarClient = sqlSugarClient;
        _dataPermissionService = dataPermissionService;
        _dataPermissionFactory = new DataPermissionFactory(dataPermissions);
    }

    /// <summary>
    /// 读取查询过滤器
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="resourceKey">资源Key</param>
    /// <param name="appId">应用Id</param>
    /// <returns></returns>
    public async Task<IList<QueryFilterGroup>?> GetQueryFilterGroupList(
        string userId,
        string resourceKey,
        string? appId
    )
    {
        var result = await _dataPermissionService.GetPolicyQueryFilterGroupsAsync(userId, resourceKey, appId);
        if (result.Count == 0)
        {
            return new List<QueryFilterGroup>();
        }

        // 基础权限列表
        var basicList = new List<string>
        {
            // 当前登录用户
            "{SelfUserId}",
            // 当前登录用户所在组织
            "{SelfOrganizationId}",
            // 当前登录用户所在组织的下级组织
            "{SelfOrganizationChildrenIds}",
        };
        var hasSelfOrganizationId = false;
        var hasSelfOrganizationChildrenIds = false;
        var extraSqlList = new List<string>();

        // 处理占位符
        foreach (var group in result)
        {
            foreach (var filter in group.Filters)
            {
                // 如果包含基础的占位符
                if (basicList.Any(key => key == filter.Value))
                {
                    switch (filter.Value)
                    {
                        // 当前登录人
                        case "{SelfUserId}":
                            filter.Value = userId;
                            break;
                        // 当前登录人部门
                        case "{SelfOrganizationId}":
                            hasSelfOrganizationId = true;
                            break;
                        // 当前登录人部门及下级部门
                        case "{SelfOrganizationChildrenIds}":
                            hasSelfOrganizationChildrenIds = true;
                            break;
                    }

                    continue;
                }

                // 动态查询条件
                var dataPermission = _dataPermissionFactory.GetInstances()
                    .Where(p => p.Key == filter.Key)
                    .FirstOrDefault(p => p.Value == filter.Value);
                if (dataPermission == null)
                {
                    continue;
                }

                // 获取查询的SQL
                extraSqlList.Add(dataPermission.GetSqlString());
            }
        }

        // 有本部门查询条件
        IList<string>? selfOrganizationIds = null;
        // 有本部门及下级部门查询条件
        IList<string>? selfOrganizationChildrenIds = null;
        // 读取基本权限
        if (hasSelfOrganizationId)
        {
            selfOrganizationIds = await _dataPermissionService.GetUserOrganizationIdsAsync(userId, appId);
        }

        if (hasSelfOrganizationChildrenIds)
        {
            selfOrganizationChildrenIds =
                await _dataPermissionService.GetUserOrganizationIdsTreeAsync(userId, appId);
        }

        List<(string Id, string MapKey)>? dynamicList = null;
        if (extraSqlList.Count > 0)
        {
            // 构建查询表达式
            var sqlObjs = extraSqlList
                .Select(sql => _sqlSugarClient.SqlQueryable<dynamic>(sql))
                .ToList();

            // 从数据库中读取动态列表，as1为值，as2为Key
            dynamicList = await _sqlSugarClient
                .UnionAll(sqlObjs)
                .Select<(string Id, string MapKey)>("Id,MapKey")
                .ToListAsync();
        }

        var newResult = new List<QueryFilterGroup>();
        // 处理占位符数据
        foreach (var group in result)
        {
            var newGroup = new QueryFilterGroup
            {
                XOR = group.XOR,
            };
            var newFilters = new List<QueryFilter>();
            foreach (var filter in group.Filters)
            {
                switch (filter)
                {
                    // 设置了组织权限且查询到了数据
                    case {Value: "{SelfOrganizationId}", Key: "OrganizationId"} when selfOrganizationIds != null:
                        // 展开查询
                        // newFilters.AddRange(selfOrganizationIds
                        //     .Select(organizationId => new QueryFilter
                        //     {
                        //         Key = "OrganizationalUnitId",
                        //         Operator = "contains",
                        //         Value = organizationId,
                        //         XOR = "or"
                        //     }));
                        newFilters.Add(new QueryFilter
                        {
                            Key = "OrganizationalUnitId",
                            Operator = "in",
                            Value = string.Join(",", selfOrganizationIds),
                            XOR = "or"
                        });

                        newFilters.Add(new QueryFilter
                        {
                            Key = "Id",
                            Operator = "boa_inner_join",
                            Value = string.Join(",", selfOrganizationIds),
                            XOR = "or",
                            ExtendFuncMethodName = "FormatInnerJoin"
                        });
                        // // 生成sql
                        // var businessSql0 = _sqlSugarClient.Queryable<OrganizationalUnitAuth>()
                        //     .AS("business_org_auths")
                        //     .Where(p => selfOrganizationIds.Contains(p.OrganizationalUnitId))
                        //     .Where(p => p.BusinessTable == resourceKey)
                        //     .Select(p => p.BusinessId)
                        //     .ToSqlString();
                        //
                        // newFilters.Add(new QueryFilter
                        // {
                        //     Key = "Id",
                        //     Operator = "sub_query",
                        //     Value = businessSql0,
                        //     XOR = "or"
                        // });
                        continue;
                    // 设置了组织权限，但是没有组织数据
                    case {Value: "{SelfOrganizationId}", Key: "OrganizationId"}:
                        newFilters.Add(new QueryFilter
                        {
                            Key = "OrganizationalUnitId",
                            Operator = "==",
                            Value = "false",
                            XOR = "and"
                        });
                        continue;
                    // 设置了组织权限且查询到了数据
                    case {Value: "{SelfOrganizationChildrenIds}", Key: "OrganizationId"}
                        when selfOrganizationChildrenIds != null:
                        // newFilters.AddRange(selfOrganizationChildrenIds
                        //     .Select(organizationId => new QueryFilter
                        //     {
                        //         Key = "OrganizationalUnitId",
                        //         Operator = "contains",
                        //         Value = organizationId,
                        //         XOR = "or"
                        //     }));
                        newFilters.Add(new QueryFilter
                        {
                            Key = "OrganizationalUnitId",
                            Operator = "in",
                            Value = string.Join(",", selfOrganizationChildrenIds),
                            XOR = "or"
                        });

                        newFilters.Add(new QueryFilter
                        {
                            Key = "Id",
                            Operator = "boa_inner_join",
                            Value = string.Join(",", selfOrganizationChildrenIds),
                            XOR = "or",
                            ExtendFuncMethodName = "FormatInnerJoin"
                        });
                        // // 生成sql
                        // var businessSql = _sqlSugarClient.Queryable<OrganizationalUnitAuth>()
                        //     .AS("business_org_auths")
                        //     .Where(p => selfOrganizationChildrenIds.Contains(p.OrganizationalUnitId))
                        //     .Where(p => p.BusinessTable == resourceKey)
                        //     .Select(p => p.BusinessId)
                        //     .ToSqlString();
                        //
                        // newFilters.Add(new QueryFilter
                        // {
                        //     Key = "Id",
                        //     Operator = "sub_query",
                        //     Value = businessSql,
                        //     XOR = "or"
                        // });
                        continue;
                    // 设置了组织权限，但是没有组织数据
                    case {Value: "{SelfOrganizationChildrenIds}", Key: "OrganizationId"}:
                        newFilters.Add(new QueryFilter
                        {
                            Key = "OrganizationalUnitId",
                            Operator = "==",
                            Value = "false",
                            XOR = "and"
                        });
                        continue;
                }

                if (dynamicList == null)
                {
                    newFilters.Add(filter);
                    continue;
                }

                var mapKey = $"{filter.Key},{filter.Value}";
                // 分组
                var groupList = dynamicList.Where(p => p.MapKey == mapKey).ToList();
                if (groupList.Count == 0)
                {
                    newFilters.Add(filter);
                    continue;
                }

                // 去重赋值
                var list = groupList.Select(p => p.Id).Distinct().ToList();
                if (filter.Operator == "in")
                {
                    filter.Value = string.Join(",", list);
                    newFilters.Add(filter);
                }
                else
                {
                    newFilters.AddRange(list.Select(id => new QueryFilter
                    {
                        Key = filter.Key,
                        Operator = filter.Operator,
                        Value = id,
                        XOR = "or"
                    }));
                }
            }

            newGroup.Filters = newFilters;
            newResult.Add(newGroup);
        }

        return newResult;
    }
}