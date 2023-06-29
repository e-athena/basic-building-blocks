namespace Athena.Infrastructure.QiNiuCloud;

/// <summary>
/// 七牛云配置
/// </summary>
public class QiNiuConfig
{
    /// <summary>
    /// KEY
    /// </summary>
    public string AccessKey { set; get; } = string.Empty;

    /// <summary>
    /// Secret
    /// </summary>
    public string SecretKey { set; get; } = string.Empty;

    /// <summary>
    /// Bucket
    /// </summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// 上传策略有效期(对应于生成的凭证的有效期)
    /// </summary>
    public int PolicyExpires { get; set; } = 3600;

    /// <summary>
    /// CdnAddress
    /// </summary>
    public string CdnAddress { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public QiNiuConfig()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessKey">AccessKey</param>
    /// <param name="secretKey">SecretKey</param>
    /// <param name="bucket">Bucket</param>
    /// <param name="cdnAddress"></param>
    public QiNiuConfig(string accessKey, string secretKey, string bucket, string cdnAddress)
    {
        AccessKey = accessKey;
        SecretKey = secretKey;
        Bucket = bucket;
        CdnAddress = cdnAddress;
    }
}