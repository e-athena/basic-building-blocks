using Athena.Infrastructure.Files.Models;

namespace Athena.Infrastructure.Files;

/// <summary>
///
/// </summary>
public interface IFileObjectService
{
    /// <summary>
    /// 上传
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <param name="stream"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    Task<FileObject> UploadAsync(string bucketName, string fileName, Stream stream, string contentType);

    /// <summary>
    /// 重命名
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    Task<FileObject> RenameAsync(string bucketName, string oldName, string newName);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task DeleteAsync(string bucketName, string fileName);

    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="sourceName"></param>
    /// <param name="targetName"></param>
    /// <returns></returns>
    Task<FileObject> CopyAsync(string bucketName, string sourceName, string targetName);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<Stream> DownloadAsync(string bucketName, string fileName);

    /// <summary>
    /// 修改元数据
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    Task<FileObject> UpdateAsync(string bucketName, string fileName, string contentType);

    /// <summary>
    /// 读取元数据
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<FileObject> GetAsync(string bucketName, string fileName);

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="prefix"></param>
    /// <param name="marker"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<FileObject>?> ListAsync(string bucketName, string prefix = null, string marker = null, int limit = 1000);
}