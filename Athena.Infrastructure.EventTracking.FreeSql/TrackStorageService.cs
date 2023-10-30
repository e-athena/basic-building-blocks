using Athena.Infrastructure.DistributedLocks;
using Athena.Infrastructure.EventTracking.Enums;
using Athena.Infrastructure.EventTracking.Messaging.Models;
using Athena.Infrastructure.EventTracking.Messaging.Requests;
using Athena.Infrastructure.EventTracking.Messaging.Responses;

namespace Athena.Infrastructure.EventTracking.FreeSql;

public class TrackStorageService : ITrackStorageService
{
    private readonly ILogger<TrackStorageService> _logger;
    private readonly IFreeSql<IEventTrackingFreeSql> _freeSql;
    private readonly IDistributedLock _distributedLock;

    public TrackStorageService(IFreeSql<IEventTrackingFreeSql> freeSql, ILoggerFactory loggerFactory,
        IDistributedLock distributedLock)
    {
        _logger = loggerFactory.CreateLogger<TrackStorageService>();
        _freeSql = freeSql;
        _distributedLock = distributedLock;
    }

    public async Task WriteAsync(Track track, CancellationToken cancellationToken = default)
    {
        // 根据TraceId查询该追踪是否已经初始化
        checkInitStatus:
        var isInit = await _freeSql.Queryable<Track>()
            .Where(p => p.TraceId == track.TraceId)
            .AnyAsync(cancellationToken);
        // 如果未初始化则初始化追踪配置
        if (!isInit)
        {
            const string resourceName = "TrackStorageCheckInitStatus";
            // 获取一个锁，用于防止多个服务请求同时写入同一个TraceId的追踪
            var lockResource = await _distributedLock.TryGetLockAsync(resourceName, track.TraceId);
            // 如果获取不到锁，则因为有别的处理正在初始化追踪信息，直接跳转至isInit重新读取配置
            if (lockResource == null)
            {
                _logger.LogDebug("获取锁失败，跳转至isInit重新读取配置，TraceId：{TraceId}", track.TraceId);
                if (EnvironmentHelper.IsDevelopment)
                {
                    Console.WriteLine("[TrackStorage:WriteAsync]获取锁失败，跳转至isInit重新读取配置，TraceId：{0}", track.TraceId);
                }

                goto checkInitStatus;
            }

            // 根据事件类型全名读取追踪配置上级ID为空的配置
            var config = await _freeSql.Queryable<TrackConfig>()
                .Where(p => p.EventTypeFullName == track.EventTypeFullName)
                .FirstAsync(p => new
                {
                    p.Id,
                    p.ParentId
                }, cancellationToken);

            // 如果找不到，则跳过记录
            if (config == null)
            {
                _logger.LogWarning("未配置事件类型 {EventTypeFullName} 的追踪配置", track.EventTypeFullName);
                // 释放锁
                await lockResource.ReleaseAsync();
                return;
            }

            // 读取配置
            var configs = await _freeSql.Queryable<TrackConfig>()
                .HasWhere(config.ParentId == null, p => p.ConfigId == config.Id || p.Id == config.Id)
                .HasWhere(config.ParentId != null, p => p.ParentPath!.Contains(config.Id) || p.Id == config.Id)
                .ToListAsync(cancellationToken);

            if (configs.Count == 0)
            {
                _logger.LogWarning("未配置事件类型 {EventTypeFullName} 的追踪配置", track.EventTypeFullName);
                // 释放锁
                await lockResource.ReleaseAsync();
                return;
            }

            // 如果当前请求对应的配置不是根节点，则需要将根节点配置加入到配置列表中
            if (config.ParentId != null)
            {
                // 读取一级配置，然后将一级配置对应的类型作为根节点
                var rootConfig = configs.First(p => p.ParentId == config.ParentId);
                // 添加根信息
                configs.Add(new TrackConfig
                {
                    Id = rootConfig.ParentId!,
                    EventName = rootConfig.EventName,
                    EventTypeName = rootConfig.EventTypeName,
                    EventTypeFullName = rootConfig.EventTypeFullName,
                });
            }

            // 初始化链路数据
            var entities = new List<Track>();
            // 转换链路数据
            ConvertTrack(configs, entities, track);

            if (!entities.Any())
            {
                // 释放锁
                await lockResource.ReleaseAsync();
                return;
            }

            // 写入链路数据
            await _freeSql.Insert(entities).ExecuteAffrowsAsync(cancellationToken);
            // 释放锁
            await lockResource.ReleaseAsync();
            return;
        }

        // 根据事件类型全名读取已配置的追踪信息
        var entity = await _freeSql.Queryable<Track>()
            .Where(p => p.EventTypeFullName == track.EventTypeFullName)
            .Where(p => p.ProcessorFullName == track.ProcessorFullName)
            // 同一个TraceId下
            .Where(p => p.TraceId == track.TraceId)
            .FirstAsync(cancellationToken);

        // 如果状态相同则跳过
        if (track.TrackStatus == entity.TrackStatus)
        {
            return;
        }

        entity.TrackStatus = track.TrackStatus;
        switch (track.TrackStatus)
        {
            case TrackStatus.Success:
                entity.EndExecuteTime = track.EndExecuteTime;
                break;
            case TrackStatus.Fail:
                entity.EndExecuteTime = track.EndExecuteTime;
                entity.ExceptionMessage = track.ExceptionMessage;
                entity.ExceptionInnerMessage = track.ExceptionInnerMessage;
                entity.ExceptionInnerType = track.ExceptionInnerType;
                entity.ExceptionStackTrace = track.ExceptionStackTrace;
                break;
            case TrackStatus.Executing:
                entity.BeginExecuteTime = track.BeginExecuteTime;
                entity.Payload = track.Payload;
                entity.ProcessorFullName = track.ProcessorFullName;
                entity.ExecuteAppName = track.ExecuteAppName;
                break;
        }

        await _freeSql.Update<Track>()
            .SetSource(entity)
            .ExecuteAffrowsAsync(cancellationToken);
    }


    public Task<Paging<GetTrackPagingResponse>> GetPagingAsync(GetTrackPagingRequest request)
    {
        return _freeSql.Queryable<Track>()
            .Where(p => p.ParentId == null || p.TrackStatus == TrackStatus.Fail)
            .HasWhere(request.Keyword, p =>
                p.TraceId == request.Keyword ||
                p.BusinessId == request.Keyword
            )
            .OrderByDescending(p => p.TrackStatus)
            .OrderByDescending(p => p.CreatedOn)
            .ToPagingAsync(request, p => new GetTrackPagingResponse());
    }

    public async Task<GetTrackInfoResponse?> GetAsync(string id)
    {
        var info = await _freeSql.Queryable<Track>()
            .Where(p => p.Id == id)
            .FirstAsync<GetTrackInfoResponse>();

        return info;
    }

    public async Task<DecompositionTreeGraphModel?> GetDecompositionTreeGraphAsync(string traceId)
    {
        var list = await _freeSql.Queryable<Track>()
            .Where(p => p.TraceId == traceId)
            .ToListAsync();

        if (list.Count == 0)
        {
            return null;
        }

        var results = new List<DecompositionTreeGraphModel>();
        GetTreeChildren(list, results);
        return results.FirstOrDefault();
    }

    public Task<int> DeleteAsync(string traceId, CancellationToken cancellationToken = default)
    {
        return _freeSql.Delete<Track>()
            .Where(p => p.TraceId == traceId)
            .ExecuteAffrowsAsync(cancellationToken);
    }

    /// <summary>
    /// 递归读取
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="results"></param>
    /// <param name="parentId"></param>
    private static void GetTreeChildren(IList<Track> entities,
        ICollection<DecompositionTreeGraphModel> results,
        string? parentId = null)
    {
        IList<Track> result = string.IsNullOrEmpty(parentId)
            ? entities.Where(p => string.IsNullOrEmpty(p.ParentId)).ToList()
            : entities.Where(p => p.ParentId == parentId).ToList();

        foreach (var item in result)
        {
            List<DecompositionTreeGraphValueItem> items;
            if (parentId == null)
            {
                // 读取开始执行时间
                var beginTime = entities
                    .Where(p => p.BeginExecuteTime.HasValue)
                    .MinBy(p => p.BeginExecuteTime)?.BeginExecuteTime!.Value;
                // 读取执行完成时间
                var endTime = entities
                    .Where(p => p.EndExecuteTime.HasValue)
                    .MaxBy(p => p.EndExecuteTime)?.EndExecuteTime!.Value;

                string? ts = null;
                if (beginTime.HasValue && endTime.HasValue)
                {
                    // 执行耗时，小于1000按ms计算，大于1000按s计算,大于60s按m计算,大于60m按h计算,大于24h按d计算,大于30d按月计算,大于12月按年计算
                    var timeSpan = endTime - beginTime;
                    ts = timeSpan switch
                    {
                        _ when timeSpan.Value.TotalMilliseconds < 1000 => $"{timeSpan.Value.TotalMilliseconds}毫秒",
                        _ when timeSpan.Value.TotalSeconds < 60 => $"{timeSpan.Value.TotalSeconds}秒",
                        _ when timeSpan.Value.TotalMinutes < 60 => $"{timeSpan.Value.TotalMinutes}分钟",
                        _ when timeSpan.Value.TotalHours < 24 => $"{timeSpan.Value.TotalHours}小时",
                        _ when timeSpan.Value.TotalDays < 30 => $"{timeSpan.Value.TotalDays}天",
                        _ when timeSpan.Value.TotalDays < 365 => $"{timeSpan.Value.TotalDays / 30}月",
                        _ => $"{timeSpan.Value.TotalDays / 365}年"
                    };
                }

                items = new List<DecompositionTreeGraphValueItem>
                {
                    new()
                    {
                        Text = "执行总耗时",
                        Value = ts ?? "-"
                    },
                    new()
                    {
                        Text = "开始执行时间",
                        Value = beginTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"
                    },
                    new()
                    {
                        Text = "执行完成时间",
                        Value = endTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"
                    },
                };
            }
            else
            {
                string? ts = null;
                if (item is {BeginExecuteTime: not null, EndExecuteTime: not null})
                {
                    // 执行耗时，小于1000按ms计算，大于1000按s计算,大于60s按m计算,大于60m按h计算,大于24h按d计算,大于30d按月计算,大于12月按年计算
                    var timeSpan = item.EndExecuteTime - item.BeginExecuteTime;
                    ts = timeSpan switch
                    {
                        _ when timeSpan.Value.TotalMilliseconds < 1000 => $"{timeSpan.Value.TotalMilliseconds}毫秒",
                        _ when timeSpan.Value.TotalSeconds < 60 => $"{timeSpan.Value.TotalSeconds}秒",
                        _ when timeSpan.Value.TotalMinutes < 60 => $"{timeSpan.Value.TotalMinutes}分钟",
                        _ when timeSpan.Value.TotalHours < 24 => $"{timeSpan.Value.TotalHours}小时",
                        _ when timeSpan.Value.TotalDays < 30 => $"{timeSpan.Value.TotalDays}天",
                        _ when timeSpan.Value.TotalDays < 365 => $"{timeSpan.Value.TotalDays / 30}月",
                        _ => $"{timeSpan.Value.TotalDays / 365}年"
                    };
                }

                items = item.TrackStatus == TrackStatus.NotExecute
                    ? new List<DecompositionTreeGraphValueItem>
                    {
                        new()
                        {
                            Text = "执行状态",
                            Value = item.TrackStatus.ToDescription()
                        }
                    }
                    : new List<DecompositionTreeGraphValueItem>
                    {
                        new()
                        {
                            Text = "执行状态",
                            Value = item.TrackStatus.ToDescription()
                        },
                        new()
                        {
                            Text = "执行耗时",
                            Value = ts ?? "-"
                        },
                        new()
                        {
                            Text = "开始执行时间",
                            Value = item.BeginExecuteTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"
                        },
                        new()
                        {
                            Text = "执行完成时间",
                            Value = item.EndExecuteTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"
                        },
                        new()
                        {
                            Text = "执行服务名称",
                            Value = item.ExecuteAppName
                        },
                    };
            }

            var res = new DecompositionTreeGraphModel
            {
                Id = item.Id,
                Value = new DecompositionTreeGraphValue
                {
                    Title = parentId != null && item.EventType.HasValue
                        ? $"[{item.EventType.Value.ToDescription()}]{item.EventName}"
                        : item.EventName,
                    Items = items
                }
            };
            if (entities.Any(p => p.ParentId == item.Id))
            {
                res.Children ??= new List<DecompositionTreeGraphModel>();
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
    /// <param name="track"></param>
    /// <param name="parentId"></param>
    /// <param name="trackParentId"></param>
    private static void ConvertTrack(IList<TrackConfig> entities,
        ICollection<Track> results,
        Track track,
        string? parentId = null,
        string? trackParentId = null)
    {
        IList<TrackConfig> result = string.IsNullOrEmpty(parentId)
            ? entities.Where(p => string.IsNullOrEmpty(p.ParentId)).ToList()
            : entities.Where(p => p.ParentId == parentId).ToList();

        foreach (var p in result)
        {
            // 检查，如果事件类型全名匹配到，则设置当前执行状态
            var isMatch =
                p.EventTypeFullName == track.EventTypeFullName &&
                p.ProcessorFullName == track.ProcessorFullName;
            var res = new Track
            {
                ParentId = trackParentId,
                BusinessId = parentId == null ? track.BusinessId : null,
                TraceId = track.TraceId,
                EventType = p.EventType,
                EventName = p.EventName,
                EventTypeFullName = p.EventTypeFullName,
                TrackStatus = isMatch ? track.TrackStatus : TrackStatus.NotExecute,
                BeginExecuteTime = isMatch ? track.BeginExecuteTime : null,
                EndExecuteTime = isMatch ? track.EndExecuteTime : null,
                Payload = isMatch ? track.Payload : null,
                ProcessorFullName = p.ProcessorFullName,
                ExceptionMessage = isMatch ? track.ExceptionMessage : null,
                ExceptionInnerMessage = isMatch ? track.ExceptionInnerMessage : null,
                ExceptionInnerType = isMatch ? track.ExceptionInnerType : null,
                ExceptionStackTrace = isMatch ? track.ExceptionStackTrace : null,
                ExecuteAppName = track.ExecuteAppName
            };
            if (entities.Any(c => c.ParentId == p.Id))
            {
                ConvertTrack(entities, results, track, p.Id, res.Id);
            }

            results.Add(res);
        }
    }
}