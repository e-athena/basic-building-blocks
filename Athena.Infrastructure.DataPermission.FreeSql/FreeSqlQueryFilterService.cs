using Athena.Infrastructure.Caching;
using Athena.Infrastructure.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Athena.Infrastructure.DataPermission.FreeSql;

/// <summary>
/// 
/// </summary>
public class FreeSqlQueryFilterService : QueryFilterServiceBase, IQueryFilterService
{
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<FreeSqlQueryFilterService> _logger;
    private readonly DataPermissionConfig _config;


    public FreeSqlQueryFilterService(
        ICacheManager cacheManager,
        ILoggerFactory loggerFactory,
        IFreeSql freeSql,
        IEnumerable<IDataPermission> dataPermissions,
        IDataPermissionService dataPermissionService
    ) : base(freeSql, dataPermissions, dataPermissionService)
    {
        _cacheManager = cacheManager;
        _logger = loggerFactory.CreateLogger<FreeSqlQueryFilterService>();
        _config = AthenaProvider.GetService<IOptions<DataPermissionConfig>>()?.Value ??
                  new DataPermissionConfig();
    }

    /// <summary>
    /// 读取查询过滤器
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<IList<QueryFilterGroup>?> GetAsync(string userId, Type type)
    {
        var resourceKey = type.Name;
        string? appId = null;
        // 不启用缓存
        if (!_config.IsEnableCache)
        {
            return await GetQueryFilterGroupList(userId, resourceKey, appId);
        }

        // 缓存Key
        var cacheKey = string.Format(_config.CacheKeyFormat, userId, resourceKey);
        // 缓存过期时间
        var expireTime = TimeSpan.FromSeconds(_config.CacheExpireSeconds);
        // 读取缓存，如果缓存中存在，则直接返回，否则从数据库中读取，并写入缓存
        var result = await _cacheManager.GetOrCreateAsync(
            cacheKey,
            async () => await GetQueryFilterGroupList(userId, resourceKey, appId),
            expireTime) ?? new List<QueryFilterGroup>();
        foreach (var group in result)
        {
            // 检查是否存在对应的字段
            foreach (var newFilter in group.Filters)
            {
                var property = type.GetProperty(newFilter.Key);
                if (property == null)
                {
                    _logger.LogWarning("字段{Key}不存在", newFilter.Key);
                }
            }
        }

        return result;
    }
}