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
    /// <param name="replaceValue">分割符，默认点号(.)</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static string ConvertToLower(string value, string replaceValue = ".")
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (!char.IsUpper(c) || i <= 0)
            {
                continue;
            }

            value = value.Insert(i, replaceValue);
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
        return ConvertToLower(value);
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

    /// <summary>
    /// 转成大写并添加下划线
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToUpperAndAddUnderline(this string value)
    {
        return ConvertToUpper(value, "_");
    }

    /// <summary>
    /// 将英文大写字母转为小写字母并在前面加上替换值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="replaceValue"></param>
    /// <returns></returns>
    public static string ConvertToUpper(string value, string replaceValue)
    {
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (!char.IsUpper(c) || i <= 0)
            {
                continue;
            }

            value = value.Insert(i, replaceValue);
            i++;
        }

        return value.ToUpper();
    }
}