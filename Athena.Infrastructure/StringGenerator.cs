namespace Athena.Infrastructure;

/// <summary>
/// 字符串生成器
/// </summary>
public static class StringGenerator
{
    private static readonly Random Rnd = new(DateTime.UtcNow.Millisecond);

    private static readonly char[] AllowableChars =
        "!@#$%&*()abcdefghjkmnpqrstuvmxyABCDEFGHJKMNPQRSTUVWXYZ123456789".ToCharArray();

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="length">长度</param>
    /// <returns></returns>
    public static string Generate(int length)
    {
        var result = new char[length];
        lock (Rnd)
        {
            for (var i = 0; i < length; i++)
            {
                result[i] = AllowableChars[Rnd.Next(0, AllowableChars.Length)];
            }
        }

        return new string(result);
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="sourceChars">源字符</param>
    /// <param name="length">长度</param>
    /// <returns></returns>
    public static string Generate(string sourceChars, int length)
    {
        var result = new char[length];
        lock (Rnd)
        {
            for (var i = 0; i < length; i++)
            {
                result[i] = sourceChars[Rnd.Next(0, AllowableChars.Length)];
            }
        }

        return new string(result);
    }
}