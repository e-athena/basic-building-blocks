using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Athena.Infrastructure.Exceptions;

namespace Athena.Infrastructure.Helpers;

/// <summary>
/// 动态码帮助类
/// </summary>
public class DynamicCodeHelper
{
    //动态码格式：业务ID+时间戳+签名
    /// <summary>
    /// 生成Code
    /// </summary>
    /// <param name="body"></param>
    /// <param name="key"></param>
    /// <param name="diff"></param>
    /// <returns></returns>
    public static string GenerateCode(string body, string key, int diff = 60)
    {
        //code=body+ts+sign
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + diff;
        var sign = Sign(body + ts, key);
        var source = $"{body},{ts},{sign}";
        return source.EncodeAsBase32String();
    }

    /// <summary>
    /// 根据Code验证有效期
    /// </summary>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="diff"></param>
    /// <returns></returns>
    public static string GetBody(string code, string key, int diff = 60)
    {
        // 检查code是否合法
        var source = code.DecodeFromBase32String();
        var arr = source.Split(',');
        if (arr.Length != 3)
        {
            throw FriendlyException.Of("二维码不正确");
        }

        // 读取ts,检查是否过期
        var ts = long.Parse(arr[1]);
        var exDiff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ts;
        if (exDiff > diff)
        {
            throw FriendlyException.Of("二维码已失效");
        }

        // 读取body+ts,然后调用Sign验证签名
        var body = $"{arr[0]}{arr[1]}";
        var sign = arr[2];
        var flag = CheckSign(body, key, sign);
        if (!flag)
        {
            throw FriendlyException.Of("二维码不正确");
        }

        return arr[0];
    }

    /// <summary>
    /// 检查签名
    /// </summary>
    /// <param name="body"></param>
    /// <param name="key"></param>
    /// <param name="sourceSign"></param>
    /// <returns></returns>
    public static bool CheckSign(string body, string key, string sourceSign)
    {
        var sign = Sign(body, key);
        return sign == sourceSign;
    }

    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="body"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Sign(string body, string key)
    {
        var keyByte = Encoding.UTF8.GetBytes(key);
        using var hmacSha256 = new HMACSHA1(keyByte);
        var hashMessage = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(body));
        // var sign = $"{hashMessage.ToBase32String()[..6]}";
        var base32 = hashMessage.ToBase32String();
        // 取中间4位
        return GetMiddleFour(base32);
    }

    private static string GetMiddleFour(string longString)
    {
        var startIndex = longString.Length / 2 - 2;
        var middleFour = longString.Substring(startIndex, 4);
        return middleFour;
    }
}