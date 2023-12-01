using Athena.Infrastructure.EventStorage.Messaging.Requests;
using Athena.Infrastructure.EventStorage.Messaging.Responses;
using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.EventStorage;

/// <summary>
/// 事件流查询服务接口
/// </summary>
public interface IEventStreamQueryService
{
    /// <summary>
    /// 读取分页列表
    /// </summary>
    /// <param name="request">请求类</param>
    /// <returns></returns>
    Task<Paging<GetEventStreamPagingResponse>> GetPagingAsync(GetEventStreamPagingRequest request);
}