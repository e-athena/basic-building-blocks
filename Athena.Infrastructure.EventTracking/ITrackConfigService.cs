using Athena.Infrastructure.EventTracking.Messaging.Requests;
using Athena.Infrastructure.EventTracking.Messaging.Responses;
using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.EventTracking;

/// <summary>
/// 追踪配置服务接口
/// </summary>
public interface ITrackConfigService
{
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveAsync(SaveTrackConfigRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取分页列表
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Paging<GetTrackConfigPagingResponse>> GetPagingAsync(GetTrackConfigPagingRequest request);

    /// <summary>
    /// 读取详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<GetTrackConfigInfoResponse?> GetAsync(string id);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> DeleteAsync(string id, CancellationToken cancellationToken = default);
}