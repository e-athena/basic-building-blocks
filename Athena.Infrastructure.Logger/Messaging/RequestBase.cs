namespace Athena.Infrastructure.Logger.Messaging;

/// <summary>
/// 请求基础
/// </summary>
public class RequestBase
{
    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = null!;

    // /// <summary>
    // /// 月份，格式：2022-09，默认当前月
    // /// </summary>
    // public string Month { get; set; } = DateTime.Now.ToString("yyyy-MM");

    /// <summary>
    /// 时间范围
    /// </summary>
    public IList<DateTime> DateRange { get; set; } = new List<DateTime>
    {
        DateTime.Now.Date,
        DateTime.Now.Date.AddDays(1)
    };
}