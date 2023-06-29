namespace Athena.Infrastructure.Helpers;

/// <summary>
/// String helper class
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// 将英文大写字母转为小写字母并在前面加上符号
    /// </summary>
    /// <remarks>UserCreatedEvent -> user.created.event</remarks>
    /// <param name="value">值</param>
    /// <param name="point">分割符，默认点号(.)</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string ConvertToLower(string value, string point)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (!char.IsUpper(c) || i <= 0)
            {
                continue;
            }

            value = value.Insert(i, point);
            i++;
        }

        return value.ToLower();
    }

    /// <summary>
    /// 将英文大写字母转为小写字母并在前面加上符号
    /// </summary>
    /// <remarks>UserCreatedEvent -> user.created.event</remarks>
    /// <param name="value">值</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string ConvertToLowerAndAddPoint(string value)
    {
        return ConvertToLower(value, ".");
    }

    /// <summary>
    /// 转成小写并添加下划线
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string ToLowerAndAddUnderline(this string value)
    {
        return ConvertToLower(value, "_");
    }
}