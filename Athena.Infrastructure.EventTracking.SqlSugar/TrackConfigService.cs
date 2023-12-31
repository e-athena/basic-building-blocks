using Athena.Infrastructure.EventTracking.Messaging.Requests;
using Athena.Infrastructure.EventTracking.Messaging.Responses;
using Athena.Infrastructure.SqlSugar;

namespace Athena.Infrastructure.EventTracking.SqlSugar;

/// <summary>
/// 
/// </summary>
public class TrackConfigService : ITrackConfigService
{
    private readonly ISqlSugarEventTrackingClient _sqlSugarClient;

    public TrackConfigService(ISqlSugarEventTrackingClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    public async Task SaveAsync(SaveTrackConfigRequest request, CancellationToken cancellationToken)
    {
        // 读取根节点
        var root = request.GetRootConfig();

        // 查询历史配置
        var configId = await QueryableNoTracking
            .Where(p => p.EventTypeFullName == root.EventTypeFullName && p.ParentId == null)
            .Select(p => p.Id)
            .FirstAsync(cancellationToken);

        if (configId != null)
        {
            // 删除原有的配置
            await _sqlSugarClient.Deleteable<TrackConfig>()
                .Where(p => p.Id == configId || p.ConfigId == configId)
                .ExecuteCommandAsync(cancellationToken);
        }

        var configs = new List<TrackConfig>();
        // 转换Id
        ConvertTrackConfig(request.Configs, configs);
        if (configs.Count != request.Configs.Count)
        {
            throw FriendlyException.Of("配置错误，转换失败");
        }

        // 批量新增
        await _sqlSugarClient.Insertable(configs).ExecuteCommandAsync(cancellationToken);
    }

    public Task<Paging<GetTrackConfigPagingResponse>> GetPagingAsync(GetTrackConfigPagingRequest request)
    {
        return QueryableNoTracking
            .Where(p => p.ParentId == null)
            .ToPagingAsync(request, p => new GetTrackConfigPagingResponse());
    }

    public async Task<GetTrackConfigInfoResponse?> GetAsync(string id)
    {
        var list = await QueryableNoTracking
            .Where(p => p.Id == id || p.ConfigId == id)
            .ToListAsync();

        if (list.Count == 0)
        {
            return null;
        }

        var results = new List<GetTrackConfigInfoResponse>();
        GetTreeChildren(list, results);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<int> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return _sqlSugarClient.Deleteable<TrackConfig>()
            .Where(p => p.Id == id || p.ConfigId == id)
            .ExecuteCommandAsync(cancellationToken);
    }

    private ISugarQueryable<TrackConfig> QueryableNoTracking => _sqlSugarClient.Queryable<TrackConfig>();

    /// <summary>
    /// 递归读取
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="results"></param>
    /// <param name="parentId"></param>
    private static void GetTreeChildren(IList<TrackConfig> entities,
        ICollection<GetTrackConfigInfoResponse> results,
        string? parentId = null)
    {
        IList<TrackConfig> result = string.IsNullOrEmpty(parentId)
            ? entities.Where(p => string.IsNullOrEmpty(p.ParentId)).ToList()
            : entities.Where(p => p.ParentId == parentId).ToList();

        foreach (var item in result)
        {
            var res = new GetTrackConfigInfoResponse
            {
                Id = item.Id,
                ConfigId = item.ConfigId,
                ParentId = item.ParentId,
                EventType = item.EventType,
                EventName = item.EventName,
                EventTypeName = item.EventTypeName,
                EventTypeFullName = item.EventTypeFullName,
                ProcessorName = item.ProcessorName,
                ProcessorFullName = item.ProcessorFullName,
            };
            if (entities.Any(p => p.ParentId == item.Id))
            {
                res.Children ??= new List<GetTrackConfigInfoResponse>();
                GetTreeChildren(entities, res.Children, item.Id);
            }

            results.Add(res);
        }
    }

    /// <summary>
    /// 递归读取
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="results"></param>
    /// <param name="parentId"></param>
    /// <param name="trackConfigParentId"></param>
    /// <param name="trackConfigPath"></param>
    /// <param name="configId"></param>
    private static void ConvertTrackConfig(IList<TrackConfig> entities,
        ICollection<TrackConfig> results,
        string? parentId = null,
        string? trackConfigParentId = null,
        string? trackConfigPath = null,
        string? configId = null)
    {
        IList<TrackConfig> result = string.IsNullOrEmpty(parentId)
            ? entities.Where(p => string.IsNullOrEmpty(p.ParentId)).ToList()
            : entities.Where(p => p.ParentId == parentId).ToList();

        foreach (var p in result)
        {
            var res = new TrackConfig
            {
                ConfigId = configId,
                ParentId = trackConfigParentId,
                ParentPath = trackConfigPath,
                EventType = p.EventType,
                EventName = p.EventName,
                EventTypeName = p.EventTypeName,
                EventTypeFullName = p.EventTypeFullName,
                ProcessorName = p.ProcessorName,
                ProcessorFullName = p.ProcessorFullName,
            };
            if (entities.Any(c => c.ParentId == p.Id))
            {
                var path = trackConfigPath == null ? res.Id : $"{trackConfigPath},{res.Id}";
                ConvertTrackConfig(entities, results, p.Id, res.Id, path, configId ?? res.Id);
            }

            results.Add(res);
        }
    }
}