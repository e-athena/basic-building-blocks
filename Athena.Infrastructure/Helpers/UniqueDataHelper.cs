namespace Athena.Infrastructure.Helpers;

/// <summary>
/// 
/// </summary>
public static class UniqueDataHelper
{
    private static readonly object Obj = new object();

    private static int GuidInt => Guid.NewGuid().GetHashCode();

    private static string GuidIntStr => Math.Abs(GuidInt).ToString();

    /// <summary>
    /// 生成
    /// </summary>
    /// <param name="mark">前缀</param>
    /// <param name="timeType">时间精确类型  1 日,2 时,3 分，4 秒(默认) </param>
    /// <param name="id">id 小于或等于0则随机生成id</param>
    /// <returns></returns>
    public static string Generate(string mark = "", int timeType = 4, int id = 0)
    {
        lock (Obj)
        {
            var number = mark;
            var ticks = (DateTime.Now.Ticks - GuidInt).ToString();

            number += GetTimeStr(timeType, out var fillCount);
            if (id > 0)
            {
                number += ticks.Substring(ticks.Length - (fillCount + 3)) + id.ToString().PadLeft(10, '0');
            }
            else
            {
                number += ticks.Substring(ticks.Length - (fillCount + 3)) + GuidIntStr.PadLeft(10, '0');
            }

            return number;
        }
    }

    /// <summary>
    /// 生成
    /// </summary>
    /// <param name="mark">前缀</param>
    /// <param name="timeType">时间精确类型  1 日,2 时,3 分，4 秒(默认)</param>
    /// <param name="id">id 小于或等于0则随机生成id</param>
    /// <returns></returns>
    public static string GenerLong(string mark, int timeType = 4, long id = 0)
    {
        lock (Obj)
        {
            var number = mark;
            var ticks = (DateTime.Now.Ticks - GuidInt).ToString();

            number += GetTimeStr(timeType, out var fillCount);
            if (id > 0)
            {
                number += ticks.Substring(ticks.Length - fillCount) + id.ToString().PadLeft(19, '0');
            }
            else
            {
                number += GuidIntStr.PadLeft(10, '0') + ticks.Substring(ticks.Length - (9 + fillCount));
            }

            return number;
        }
    }

    /// <summary>
    /// 获取时间字符串
    /// </summary>
    /// <param name="timeType">时间精确类型  1 日,2 时,3 分，4 秒(默认)</param>
    /// <param name="fillCount">填充位数</param>
    /// <returns></returns>
    private static string GetTimeStr(int timeType, out int fillCount)
    {
        var time = DateTime.Now;
        if (timeType == 1)
        {
            fillCount = 6;
            return time.ToString("yyyyMMdd");
        }

        if (timeType == 2)
        {
            fillCount = 4;
            return time.ToString("yyyyMMddHH");
        }
        if (timeType == 3)
        {
            fillCount = 2;
            return time.ToString("yyyyMMddHHmm");
        }
        fillCount = 0;
        return time.ToString("yyyyMMddHHmmss");
    }
}