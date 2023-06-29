using System.Text.RegularExpressions;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.QiNiuCloud.Helpers;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;

namespace Athena.Infrastructure.QiNiuCloud;

/// <summary>
/// 七牛对象存储服务接口实现
/// </summary>
public class QiNiuKoDoService : IQiNiuKoDoService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public QiNiuKoDoService(IOptions<QiNiuConfig> options)
    {
        Config = options.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="qiNiuConfig"></param>
    public QiNiuKoDoService(QiNiuConfig qiNiuConfig)
    {
        Config = qiNiuConfig;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public ApiResult<UploadResult> UploadForUrl(string url)
    {
        return UploadForUrl(url, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public ApiResult<UploadResult> UploadForUrl(string url, string key)
    {
        return ResultHelper.CommonResult(() =>
        {
            if (!IsUrl(url))
            {
                throw new Exception("网络地址无效,请检查");
            }

            // 读取文件流
            var stream = url.GetStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var (um, token) = GetUploadManageAndToken();
            var res = um.UploadStream(stream, key, token, new PutExtra());
            return CheckAndGetResult(res);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public Task<ApiResult<UploadResult>> UploadForUrlAsync(string url)
    {
        return UploadForUrlAsync(url, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<ApiResult<UploadResult>> UploadForUrlAsync(string url, string key)
    {
        return await Task.FromResult(UploadForUrl(url, key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public ApiResult<UploadResult> UploadData(byte[] data, string key)
    {
        return ResultHelper.CommonResult(() =>
        {
            var (um, token) = GetUploadManageAndToken();
            var res = um.UploadData(data, key, token, new PutExtra());
            return CheckAndGetResult(res);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<ApiResult<UploadResult>> UploadDataAsync(byte[] data, string key)
    {
        return await Task.FromResult(UploadData(data, key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="localFile"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public ApiResult<UploadResult> UploadFile(string localFile, string key)
    {
        return ResultHelper.CommonResult(() =>
        {
            var (um, token) = GetUploadManageAndToken();
            var res = um.UploadFile(localFile, key, token, new PutExtra());
            return CheckAndGetResult(res);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="localFile"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<ApiResult<UploadResult>> UploadFileAsync(string localFile, string key)
    {
        return await Task.FromResult(UploadFile(localFile, key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public ApiResult<UploadResult> UploadStream(Stream stream, string key)
    {
        return ResultHelper.CommonResult(() =>
        {
            var (um, token) = GetUploadManageAndToken();
            var res = um.UploadStream(stream, key, token, new PutExtra());
            return CheckAndGetResult(res);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<ApiResult<UploadResult>> UploadStreamAsync(Stream stream, string key)
    {
        return await Task.FromResult(UploadStream(stream, key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetUploadToken()
    {
        return CreateUploadToken();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetUploadTokenAsync()
    {
        return await Task.FromResult(GetUploadToken());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetDownloadUrl(string url)
    {
        var e = UnixTimestamp.GetUnixTimestamp(3600);
        var domainUrl = $"{Config.CdnAddress}/{url}?e={e}";
        var token = CreateDownloadToken(domainUrl);
        return $"/{url}?e={e}&token={token}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<string> GetDownloadUrlAsync(string url)
    {
        return await Task.FromResult(GetDownloadUrl(url));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public string GetDownloadUrlWithDomain(string url)
    {
        return $"{Config.CdnAddress}{GetDownloadUrl(url)}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<string> GetDownloadUrlWithDomainAsync(string url)
    {
        return await Task.FromResult(GetDownloadUrlWithDomain(url));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task DeleteAsync(string key)
    {
        Delete(key);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="key"></param>
    public void Delete(string key)
    {
        try
        {
            var (mac, _) = GetMacAndPolicyJson();
            var bm = GetBucketManagerInstance(mac);
            bm.Delete(Config.Bucket, key);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <returns></returns>
    public QiNiuConfig Config { get; }

    #region 私有方法

    private static UploadResult CheckAndGetResult(HttpResult res)
    {
        var result = JsonConvert.DeserializeObject<UploadResult>(res.Text);
        if (result == null)
        {
            throw new ApplicationException("无响应内容,请联系管理员。");
        }

        // 如果有错误
        if (result.HasError())
        {
            throw new ApplicationException(result.Error);
        }

        // 转成小写
        result.Ext = result.Ext.ToLower();
        return result;
    }

    private string CreateUploadToken()
    {
        var (mac, jsonStr) = GetMacAndPolicyJson();
        return Auth.CreateUploadToken(mac, jsonStr);
    }

    private string CreateDownloadToken(string url)
    {
        var mac = new Mac(Config.AccessKey, Config.SecretKey);
        return Auth.CreateDownloadToken(mac, url);
    }


    private (Mac mac, string policyJson) GetMacAndPolicyJson()
    {
        var mac = new Mac(Config.AccessKey, Config.SecretKey);
        var bucket = Config.Bucket;
        // 上传策略，参见 
        // https://developer.qiniu.com/kodo/manual/put-policy
        var putPolicy = new PutPolicy
        {
            // putPolicy.Scope = bucket + ":" + saveKey;
            // 如果需要设置为"覆盖"上传(如果云端已有同名文件则覆盖)，请使用 SCOPE = "BUCKET:KEY"
            Scope = bucket,
            SaveKey = "$(etag)$(ext)",
            ReturnBody =
                "{\"bucket\":$(bucket),\"name\":$(fname),\"key\":$(key),\"hash\":$(etag),\"size\":$(fsize),\"mimeType\":$(mimeType),\"endUser\":$(endUser),\"ext\":$(ext)}"
        };
        // 上传策略有效期(对应于生成的凭证的有效期)
        putPolicy.SetExpires(Config.PolicyExpires);
        // 生成上传凭证，参见
        // https://developer.qiniu.com/kodo/manual/upload-token            
        var jsonStr = putPolicy.ToJsonString();
        return (mac, jsonStr);
    }

    private static UploadManager GetUploadManagerInstance(Config? config = null)
    {
        return new UploadManager(config ?? new Config());
    }

    private static BucketManager GetBucketManagerInstance(Mac mac, Config? config = null)
    {
        return new BucketManager(mac, config ?? new Config());
    }

    private (UploadManager um, string token) GetUploadManageAndToken(Config? config = null)
    {
        return (GetUploadManagerInstance(config), CreateUploadToken());
    }


    /// <summary>  
    /// 验证网址（可以匹配IPv4地址但没对IPv4地址进行格式验证；IPv6暂时没做匹配）  
    /// [允许省略"://"；可以添加端口号；允许层级；允许传参；域名中至少一个点号且此点号前要有内容]  
    /// </summary>  
    /// <param name="input">待验证的字符串</param>  
    /// <returns>是否匹配</returns>  
    private static bool IsUrl(string input)
    {
        const string pattern =
            @"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?";
        return IsMatch(input, pattern);
    }

    /// <summary>  
    /// 验证字符串是否匹配正则表达式描述的规则  
    /// </summary>  
    /// <param name="inputStr">待验证的字符串</param>  
    /// <param name="patternStr">正则表达式字符串</param>  
    /// <param name="ifIgnoreCase">匹配时是否不区分大小写</param>  
    /// <param name="ifValidateWhiteSpace">是否验证空白字符串</param>  
    /// <returns>是否匹配</returns>  
    private static bool IsMatch(
        string inputStr,
        string patternStr,
        bool ifIgnoreCase = false,
        bool ifValidateWhiteSpace = false)
    {
        if (!ifValidateWhiteSpace && string.IsNullOrWhiteSpace(inputStr))
            return false; //如果不要求验证空白字符串而此时传入的待验证字符串为空白字符串，则不匹配  
        var regex = ifIgnoreCase ? new Regex(patternStr, RegexOptions.IgnoreCase) : new Regex(patternStr);
        return regex.IsMatch(inputStr);
    }

    #endregion
}