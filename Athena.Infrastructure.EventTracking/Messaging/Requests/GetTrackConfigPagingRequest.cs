using Athena.Infrastructure.Messaging.Requests;

namespace Athena.Infrastructure.EventTracking.Messaging.Requests;

/// <summary>
/// 读取配置分页请求
/// </summary>
public class GetTrackConfigPagingRequest : GetPagingRequestBase
{
    /// <summary>
    /// 排序
    /// </summary>
    public override string? Sorter { get; set; } = "a.CreatedOn DESC";
}