using System.Text.RegularExpressions;
using Athena.Infrastructure.Files;
using Athena.Infrastructure.Files.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;

namespace Athena.Infrastructure.QiNiuCloud.FileObjects;

/// <summary>
///
/// </summary>
public class FileObjectService : IFileObjectService
{
    private readonly ILogger<FileObjectService> _logger;

    /// <summary>
    ///
    /// </summary>
    public string Provider => "QiNiu";

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileObjectService(IOptions<QiNiuConfig> options, ILogger<FileObjectService> logger)
    {
        _logger = logger;
        Config = options.Value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="qiNiuConfig"></param>
    /// <param name="logger"></param>
    public FileObjectService(QiNiuConfig qiNiuConfig, ILogger<FileObjectService> logger)
    {
        Config = qiNiuConfig;
        _logger = logger;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <param name="stream"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<FileObject> UploadAsync(string bucketName, string fileName, Stream stream, string contentType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<FileObject> RenameAsync(string bucketName, string oldName, string newName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task DeleteAsync(string bucketName, string fileName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="sourceName"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<FileObject> CopyAsync(string bucketName, string sourceName, string targetName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<Stream> DownloadAsync(string bucketName, string fileName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<FileObject> UpdateAsync(string bucketName, string fileName, string contentType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<FileObject> GetAsync(string bucketName, string fileName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="prefix"></param>
    /// <param name="marker"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async Task<List<FileObject>?> ListAsync(string bucketName, string prefix = null, string marker = null,
        int limit = 1000)
    {
        var mac = new Mac(Config.AccessKey, Config.SecretKey);
        var bm = new BucketManager(mac, new Config());
        var rsp = await Task.FromResult(bm.ListFiles(bucketName, prefix, marker, limit, null));

        if (rsp.Code == 200)
        {
            return rsp.Result.Items.Select(x => new FileObject
            {
                BucketName = bucketName,
                Name = x.Key,
                Size = x.Fsize,
                MimeType = x.MimeType,
                Hash = x.Hash,
                PutTime = x.PutTime,
                StorageClass = x.FileType,
                Extend = x.EndUser
            }).ToList();
        }

        _logger.LogError("获取文件列表失败，错误码:{Code},错误信息:{Message}", rsp.Code, rsp.RefText);
        return null;

        ;
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
        return Qiniu.Util.Auth.CreateUploadToken(mac, jsonStr);
    }

    private string CreateDownloadToken(string url)
    {
        var mac = new Mac(Config.AccessKey, Config.SecretKey);
        return Qiniu.Util.Auth.CreateDownloadToken(mac, url);
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