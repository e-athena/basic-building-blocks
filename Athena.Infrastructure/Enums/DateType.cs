namespace Athena.Infrastructure.Enums;

/// <summary>
/// 日期类型
/// </summary>
public enum DateType
{
    // 1.今天，2.昨天，3.本周，4.上周，5.本月，6.上月,7.本年
    /// <summary>
    /// 今天
    /// </summary>
    [Description("今天")] Today = 1,

    /// <summary>
    /// 昨天
    /// </summary>
    [Description("昨天")] Yesterday = 2,

    /// <summary>
    /// 本周
    /// </summary>
    [Description("本周")] Week = 3,

    /// <summary>
    /// 上周
    /// </summary>
    [Description("上周")] LastWeek = 4,

    /// <summary>
    /// 本月
    /// </summary>
    [Description("本月")] Month = 5,

    /// <summary>
    /// 上月
    /// </summary>
    [Description("上月")] LastMonth = 6,

    /// <summary>
    /// 本年
    /// </summary>
    [Description("本年")] Year = 7,
}