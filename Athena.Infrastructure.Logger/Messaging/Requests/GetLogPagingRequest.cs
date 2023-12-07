using Athena.Infrastructure.Messaging.Requests;

namespace Athena.Infrastructure.Logger.Messaging.Requests;

/// <summary>
/// 
/// </summary>
public class GetLogPagingRequest : GetPagingRequestBase
{
    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>
    /// 时间范围
    /// </summary>
    public IList<DateTime> DateRange { get; set; } = new List<DateTime>
    {
        DateTime.Now.Date,
        DateTime.Now.Date.AddDays(1)
    };

    /// <summary>
    /// 用户Id
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 追踪ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 日志等级
    /// </summary>
    public LogLevel? LogLevel { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public override string? Sorter { get; set; } = "a.Id DESC";
}