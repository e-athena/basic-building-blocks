namespace Athena.Infrastructure.Helpers;

/// <summary>
/// 日期帮助类
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// 根据开始和结束时间获取月份列表
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns></returns>
    public static List<string> GetMonthList(DateTime startTime, DateTime endTime)
    {
        var list = new List<string>();
        var startMonth = startTime.ToString("yyyy-MM");
        var endMonth = endTime.ToString("yyyy-MM");
        if (startMonth == endMonth)
        {
            list.Add(startMonth);
            return list;
        }

        var startYear = startTime.Year;
        var startMonthNum = startTime.Month;
        var endYear = endTime.Year;
        var endMonthNum = endTime.Month;
        for (var i = startYear; i <= endYear; i++)
        {
            var start = i == startYear ? startMonthNum : 1;
            var end = i == endYear ? endMonthNum : 12;
            for (var j = start; j <= end; j++)
            {
                list.Add($"{i}-{j:D2}");
            }
        }

        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static DateTime NewDateTime()
    {
        return DateTimeOffset.Now.UtcDateTime;
    }

    /// <summary>
    /// 计算时间差
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public static TimeSpan DateDiff(DateTime startTime, DateTime endTime)
    {
        var ts1 = new TimeSpan(startTime.Ticks);
        var ts2 = new TimeSpan(endTime.Ticks);
        var ts = ts1.Subtract(ts2).Duration();
        return ts;
    }

    /// <summary>
    /// 计算时间差/天
    /// </summary>
    /// <param name="dateStart"></param>
    /// <param name="dateEnd"></param>
    /// <returns></returns>
    public static int DateDiffByDay(DateTime dateStart, DateTime dateEnd)
    {
        var start = Convert.ToDateTime(dateStart.ToShortDateString());
        var end = Convert.ToDateTime(dateEnd.AddDays(1).ToShortDateString());

        var sp = end.Subtract(start);

        return sp.Days > 0 ? sp.Days : 0;
    }

    /// <summary>
    /// 获取2个时间差组件的 formatter数组
    /// </summary>
    /// <param name="dateStart">开始时间</param>
    /// <param name="dateEnd">结束时间</param>
    /// <param name="formatter">格式： yyyy   yyyyMM  yyyyMMdd,转换后可以转换为int类型 </param>
    /// <returns>eg:2019-01-02~2021-03-01 formatter:yyyy  return :['2019','2020','2021']</returns>
    public static List<string> DateDiffFormatter(DateTime dateStart, DateTime dateEnd, string formatter)
    {
        var result = new List<string>();
        var startYear = int.Parse(dateStart.ToString(formatter));
        var endYear = int.Parse(dateEnd.ToString(formatter));
        for (var i = startYear; i <= endYear; i++)
        {
            var tmplTime = i.ToString();
            switch (formatter)
            {
                case "yyyyMM":
                    tmplTime = string.Concat(tmplTime.AsSpan(0, 4), "-", tmplTime.AsSpan(4, 2), "-01");
                    break;
                case "yyyyMMdd":
                    tmplTime = tmplTime.Substring(0, 4) + "-" + tmplTime.Substring(4, 2) + "-" +
                               tmplTime.Substring(6, 2);
                    break;
            }

            if (DateTime.TryParse(tmplTime, out _))
            {
                result.Add(i.ToString());
            }
        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="birthdate"></param>
    /// <returns></returns>
    public static int GetAgeByBirthdate(DateTime birthdate)
    {
        var now = DateTime.Now;
        var age = now.Year - birthdate.Year;
        if (now.Month < birthdate.Month || (now.Month == birthdate.Month && now.Day < birthdate.Day))
        {
            age--;
        }

        return age < 0 ? 0 : age;
    }

    /// <summary>
    /// 根据类型获取开始和结束时间
    /// </summary>
    /// <param name="dateType">1.今天，2.昨天，3.本周，4.上周，5.本月，6.上月</param>
    /// <param name="weekStartOnMonday">一周的开始是否为周一</param>
    /// <returns></returns>
    public static Tuple<DateTime, DateTime> GetStartAndEndTime(DateType? dateType, bool weekStartOnMonday = true)
    {
        var now = DateTime.Now; // .AddDays(7);
        var nowDate = now.Date;
        // 默认返回一个月的开始，结束时间
        var startTime = new DateTime(nowDate.Year, nowDate.Month, 1);
        var endTime = startTime.AddMonths(1);
        return dateType switch
        {
            // 今天
            DateType.Today => new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today.AddDays(1)),
            // 昨天
            DateType.Yesterday => new Tuple<DateTime, DateTime>(DateTime.Today.AddDays(-1), DateTime.Today),
            // 本周
            DateType.Week => GetWeekRange(nowDate),
            // 上周
            DateType.LastWeek => GetLastWeekRange(nowDate),
            // 本月
            DateType.Month => new Tuple<DateTime, DateTime>(startTime, endTime),
            // 上月
            DateType.LastMonth => new Tuple<DateTime, DateTime>(startTime.AddMonths(-1), startTime),
            // 本年
            DateType.Year => new Tuple<DateTime, DateTime>(
                new DateTime(nowDate.Year, 1, 1),
                new DateTime(nowDate.Year, 1, 1).AddYears(1)
            ),
            // 默认本月
            _ => new Tuple<DateTime, DateTime>(startTime, endTime)
        };
    }

    /// <summary>
    /// 指定开始时间和结束时间，获取指定星期几的日期列表
    /// </summary>
    /// <param name="dayOfWeek">星期几</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns></returns>
    public static IList<DateTime> GetDayOfWeekList(DayOfWeek dayOfWeek, DateTime startTime, DateTime endTime)
    {
        var list = new List<DateTime>();
        var time = startTime;
        while (time <= endTime)
        {
            if (time.DayOfWeek == dayOfWeek)
            {
                list.Add(time);
            }

            time = time.AddDays(1);
        }

        return list;
    }
    
    /// <summary>
    /// 根据时间读取本周周一的日期
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime GetMonday(DateTime date)
    {
        var dt = date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-1) : date;
        return dt.AddDays(-1 * (int) dt.DayOfWeek + 1);
    }

    /// <summary>
    /// 读取第几周
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int GetWeekOfYear(DateTime date)
    {
        var dt = date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-1) : date;
        return (int) Math.Ceiling((double) dt.DayOfYear / 7);
    }

    /// <summary>
    /// 读取给定日期的周开始日期和结束日期
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Tuple<DateTime, DateTime> GetWeekRange(DateTime date)
    {
        return new Tuple<DateTime, DateTime>(GetMonday(date), date);
    }

    /// <summary>
    /// 读取上周的周一到周日的日期
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static Tuple<DateTime, DateTime> GetLastWeekRange(DateTime date)
    {
        var startDay = date.DayOfWeek switch
        {
            DayOfWeek.Monday => -7,
            DayOfWeek.Tuesday => -8,
            DayOfWeek.Wednesday => -9,
            DayOfWeek.Thursday => -10,
            DayOfWeek.Friday => -11,
            DayOfWeek.Saturday => -12,
            DayOfWeek.Sunday => -13,
            _ => 0
        };

        var startDate = date.AddDays(startDay);
        var endDate = startDate.AddDays(6);

        return new Tuple<DateTime, DateTime>(startDate, endDate);
    }
}