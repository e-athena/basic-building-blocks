using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.QiNiuCloud;

/// <summary>
/// 七牛对象存储服务接口
/// </summary>
public interface IQiNiuKoDoService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url">网络地址</param>
    /// <returns></returns>
    ApiResult<UploadResult> UploadForUrl(string url);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url">网络地址</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    ApiResult<UploadResult> UploadForUrl(string url, string key);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url">网络地址</param>
    /// <returns></returns>
    Task<ApiResult<UploadResult>> UploadForUrlAsync(string url);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url">网络地址</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    Task<ApiResult<UploadResult>> UploadForUrlAsync(string url, string key);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="data">文件字节数据</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    ApiResult<UploadResult> UploadData(byte[] data, string key);

    /// <summary>
    /// [异步]上传文件
    /// </summary>
    /// <param name="data">文件字节数据</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    Task<ApiResult<UploadResult>> UploadDataAsync(byte[] data, string key);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="localFile">本地文件路径</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    ApiResult<UploadResult> UploadFile(string localFile, string key);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="localFile">本地文件路径</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    Task<ApiResult<UploadResult>> UploadFileAsync(string localFile, string key);

    /// <summary>
    /// 上传文件流
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    ApiResult<UploadResult> UploadStream(Stream stream, string key);

    /// <summary>
    /// [异步]上传文件流
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="key">Key</param>
    /// <returns></returns>
    Task<ApiResult<UploadResult>> UploadStreamAsync(Stream stream, string key);

    /// <summary>
    /// 获取上传的Token
    /// </summary>
    /// <returns></returns>
    string GetUploadToken();

    /// <summary>
    /// 获取上传的Token
    /// </summary>
    /// <returns></returns>
    Task<string> GetUploadTokenAsync();

    /// <summary>
    /// 获取下载链接（不包含域名），针对私有空间
    /// </summary>
    /// <param name="url">Key</param>
    /// <returns></returns>
    string GetDownloadUrl(string url);

    /// <summary>
    /// [异步]获取下载链接（不包含域名），针对私有空间
    /// </summary>
    /// <param name="url">Key</param>
    /// <returns></returns>
    Task<string> GetDownloadUrlAsync(string url);

    /// <summary>
    /// 获取完整下载链接，针对私有空间
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    string GetDownloadUrlWithDomain(string url);

    /// <summary>
    /// [异步]获取完整下载链接，针对私有空间
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<string> GetDownloadUrlWithDomainAsync(string url);

    /// <summary>
    /// [异步]删除资源文件
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task DeleteAsync(string key);

    /// <summary>
    /// 删除资源文件
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    void Delete(string key);

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <returns></returns>
    QiNiuConfig Config { get; }
}