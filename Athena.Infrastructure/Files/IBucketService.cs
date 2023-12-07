namespace Athena.Infrastructure.Files;

/// <summary>
///
/// </summary>
public interface IBucketService
{
    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    Task CreateAsync(string bucketName);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    Task DeleteAsync(string bucketName);

}