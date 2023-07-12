using Athena.Infrastructure.EventTracking.Messaging.Models;
using Athena.Infrastructure.EventTracking.Messaging.Requests;
using Athena.Infrastructure.EventTracking.Messaging.Responses;
using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.EventTracking;

/// <summary>
/// 追踪存储服务
/// </summary>
public interface ITrackStorageService
{
    /// <summary>
    /// 写入追踪数据
    /// </summary>
    /// <param name="track"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(Track track, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取分页数据
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Paging<GetTrackPagingResponse>> GetPagingAsync(GetTrackPagingRequest request);

    /// <summary>
    /// 读取详情
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns></returns>
    Task<GetTrackInfoResponse?> GetAsync(string id);

    /// <summary>
    /// 读取详情
    /// </summary>
    /// <param name="traceId"></param>
    /// <returns></returns>
    Task<DecompositionTreeGraphModel?> GetDecompositionTreeGraphAsync(string traceId);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="traceId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(string traceId, CancellationToken cancellationToken = default);
}