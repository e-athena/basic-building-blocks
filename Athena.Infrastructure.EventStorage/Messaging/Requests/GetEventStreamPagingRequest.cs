using System.ComponentModel.DataAnnotations;
using Athena.Infrastructure.Messaging.Requests;

namespace Athena.Infrastructure.EventStorage.Messaging.Requests;

/// <summary>
/// 读取事件流分页请求
/// </summary>
public class GetEventStreamPagingRequest : GetPagingRequestBase
{
    /// <summary>
    /// 聚合根ID
    /// </summary>
    /// <value></value>
    [MaxLength(36)]
    [Required]
    public string AggregateRootId { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public override string? Sorter { get; set; } = "a.Version DESC";
}