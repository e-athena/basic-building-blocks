using System.Security.Cryptography;
using System.Text;

namespace Athena.Infrastructure.Helpers;

/// <summary>
/// 
/// </summary>
public static class SecurityHelper
{
    private const string Key64 = "EP6IVshw"; //注意了，是8个字符，64位
    private const string Iv64 = "ri5zqgqt";
    private const string IvSuffix = "kMOVt8ld";

    #region 加密数据

    /// <summary> 
    /// 加密数据 
    /// </summary> 
    /// <param name="data">要加密数据</param> 
    /// <returns></returns> 
    public static string Encrypt(string data)
    {
        var byKey = Encoding.ASCII.GetBytes(Key64);
        var byIv = Encoding.ASCII.GetBytes(Iv64);

        var cryptoProvider = DES.Create();
        var ms = new MemoryStream();
        var cst = new CryptoStream(ms,
            cryptoProvider.CreateEncryptor(byKey, byIv),
            CryptoStreamMode.Write
        );

        var sw = new StreamWriter(cst);
        sw.Write(data);
        sw.Flush();
        cst.FlushFinalBlock();
        sw.Flush();
        return Convert.ToBase64String(ms.GetBuffer(), 0, (int) ms.Length);
    }

    /// <summary>
    /// Md5
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Md5(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        using var md5 = MD5.Create();
        var result = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
        var strResult = BitConverter.ToString(result);
        var result3 = strResult.Replace("-", "");
        return result3.ToLower();
    }

    /// <summary> 
    /// 加密数据 
    /// </summary> 
    /// <param name="data">要加密数据</param>
    /// <param name="key">KEY</param>
    /// <param name="iv">IV</param>
    /// <returns></returns> 
    public static string Encrypt(string data, string key, string iv)
    {
        var byKey = Encoding.ASCII.GetBytes(key);
        var byIv = Encoding.ASCII.GetBytes(iv);

        var cryptoProvider = DES.Create();
        var ms = new MemoryStream();
        var cst = new CryptoStream(ms,
            cryptoProvider.CreateEncryptor(byKey, byIv),
            CryptoStreamMode.Write
        );

        var sw = new StreamWriter(cst);
        sw.Write(data);
        sw.Flush();
        cst.FlushFinalBlock();
        sw.Flush();
        return Convert.ToBase64String(ms.GetBuffer(), 0, (int) ms.Length);
    }

    /// <summary> 
    /// 加密数据 
    /// </summary> 
    /// <param name="data">要加密数据</param>
    /// <param name="key">KEY</param>
    /// <returns></returns> 
    public static string Encrypt(string data, string key)
    {
        return Encrypt(data, key, $"{key}{IvSuffix}");
    }

    #endregion

    #region 解密数据

    /// <summary> 
    /// 解密数据 
    /// </summary> 
    /// <param name="data">要解密数据</param> 
    /// <returns></returns> 
    public static string? Decrypt(string data)
    {
        var byKey = Encoding.ASCII.GetBytes(Key64);
        var byIv = Encoding.ASCII.GetBytes(Iv64);

        try
        {
            var byEnc = Convert.FromBase64String(data);
            var cryptoProvider = DES.Create();
            var ms = new MemoryStream(byEnc);
            var cst = new CryptoStream(ms,
                cryptoProvider.CreateDecryptor(byKey, byIv),
                CryptoStreamMode.Read
            );
            var sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    /// <summary> 
    /// 解密数据 
    /// </summary> 
    /// <param name="data">要解密数据</param>
    /// <param name="key">KEY</param>
    /// <param name="iv">IV</param>
    /// <returns></returns> 
    public static string? Decrypt(string data, string key, string iv)
    {
        var byKey = Encoding.ASCII.GetBytes(key);
        var byIv = Encoding.ASCII.GetBytes(iv);

        try
        {
            var byEnc = Convert.FromBase64String(data);
            var cryptoProvider = DES.Create();
            var ms = new MemoryStream(byEnc);
            var cst = new CryptoStream(ms,
                cryptoProvider.CreateDecryptor(byKey, byIv),
                CryptoStreamMode.Read
            );
            var sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    /// <summary> 
    /// 解密数据 
    /// </summary> 
    /// <param name="data">要解密数据</param>
    /// <param name="key">KEY</param>
    /// <returns></returns> 
    public static string? Decrypt(string data, string key)
    {
        return Decrypt(data, key, $"{key}{IvSuffix}");
    }

    #endregion


    /// <summary>
    /// 微信加密数据解密算法
    ///     - https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/signature.html#%E5%8A%A0%E5%AF%86%E6%95%B0%E6%8D%AE%E8%A7%A3%E5%AF%86%E7%AE%97%E6%B3%95
    /// </summary>
    /// <param name="encryptedData">加密数据</param>
    /// <param name="sessionKey">对称解密秘钥</param>
    /// <param name="iv">初始向量</param>
    /// <returns></returns>
    public static string? WeChatAesDecrypt(string encryptedData, string sessionKey, string iv)
    {
        try
        {
            //16进制数据转换成byte
            var encryptedDataByte = Convert.FromBase64String(encryptedData);
            var rijndaelCipher = Aes.Create();
            rijndaelCipher.Key = Convert.FromBase64String(sessionKey);
            rijndaelCipher.IV = Convert.FromBase64String(iv);
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            var transform = rijndaelCipher.CreateDecryptor();
            var plainText = transform.TransformFinalBlock(encryptedDataByte, 0, encryptedDataByte.Length);
            var result = Encoding.Default.GetString(plainText);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    #region base32

    /// <summary> 
    /// The different characters allowed in Base32 encoding. 
    /// </summary> 
    /// <remarks> 
    /// This is a 32-character subset of the twenty-six letters A–Z and six digits 2–7. 
    /// </remarks>
    private const string Base32AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary> 
    /// Converts a byte array into a Base32 string. 
    /// </summary> 
    /// <param name="input">The string to convert to Base32.</param> 
    /// <param name="addPadding">Whether or not to add RFC3548 '='-padding to the string.</param> 
    /// <returns>A Base32 string.</returns> 
    /// <remarks> 
    /// https://tools.ietf.org/html/rfc3548#section-2.2 indicates padding MUST be added unless the reference to the RFC tells us otherswise. 
    /// https://github.com/google/google-authenticator/wiki/Key-Uri-Format indicates that padding SHOULD be omitted. 
    /// To meet both requirements, you can omit padding when required. 
    /// </remarks> 
    public static string ToBase32String(this byte[] input, bool addPadding = true)
    {
        if (input.Length == 0)
        {
            return string.Empty;
        }

        var bits = input
            .Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Aggregate((a, b) => a + b)
            .PadRight((int) (Math.Ceiling((input.Length * 8) / 5d) * 5), '0');
        var result = Enumerable.Range(0, bits.Length / 5)
            .Select(i => Base32AllowedCharacters.Substring(Convert.ToInt32(bits.Substring(i * 5, 5), 2), 1))
            .Aggregate((a, b) => a + b);
        if (addPadding)
        {
            result = result.PadRight((int) (Math.Ceiling(result.Length / 8d) * 8), '=');
        }

        return result;
    }

    /// <summary>
    /// Base32编码
    /// </summary>
    /// <param name="input"></param>
    /// <param name="addPadding"></param>
    /// <returns></returns>
    public static string EncodeAsBase32String(this string input, bool addPadding = true)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        var result = bytes.ToBase32String(addPadding);
        return result;
    }

    /// <summary>
    /// Base32解码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string DecodeFromBase32String(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var bytes = input.ToByteArray();
        var result = Encoding.UTF8.GetString(bytes);
        return result;
    }

    /// <summary>
    /// Converts a Base32 string into the corresponding byte array, using 5 bits per character.
    /// </summary>
    /// <param name="input">The Base32 String</param>
    /// <returns>A byte array containing the properly encoded bytes.</returns> 
    public static byte[] ToByteArray(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Array.Empty<byte>();
        }

        var bits = input.TrimEnd('=').ToUpper().ToCharArray()
            .Select(c => Convert.ToString(Base32AllowedCharacters.IndexOf(c), 2).PadLeft(5, '0'))
            .Aggregate((a, b) => a + b);
        var result = Enumerable.Range(0, bits.Length / 8).Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2))
            .ToArray();
        return result;
    }

    #endregion
}