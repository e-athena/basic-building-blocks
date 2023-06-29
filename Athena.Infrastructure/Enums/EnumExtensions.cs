namespace Athena.Infrastructure.Enums;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 获取枚举描述详情
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string ToDescription(this Enum enumValue)
    {
        try
        {
            var str = enumValue.ToString();
            var field = enumValue.GetType().GetField(str);
            var objs = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs == null || objs.Length == 0)
                return str;
            var da = (DescriptionAttribute)objs[0];
            return da.Description;
        }
        catch (Exception)
        {
            return "未设置";
        }
    }
}