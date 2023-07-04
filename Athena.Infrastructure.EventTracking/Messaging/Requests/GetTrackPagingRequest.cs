using Athena.Infrastructure.Messaging.Requests;

namespace Athena.Infrastructure.EventTracking.Messaging.Requests;

/// <summary>
/// 读取追踪数据分页请求
/// </summary>
public class GetTrackPagingRequest : GetPagingRequestBase
{
    /// <summary>
    /// 错误数据
    /// </summary>
    public bool? Error { get; set; }
}