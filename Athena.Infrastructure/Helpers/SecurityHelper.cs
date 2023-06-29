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
}