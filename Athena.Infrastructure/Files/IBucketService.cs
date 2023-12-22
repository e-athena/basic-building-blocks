namespace Athena.Infrastructure.Files;

/// <summary>
/// 桶服务接口
/// </summary>
public interface IBucketService
{
    /// <summary>
    /// 提供程序
    /// </summary>
    public string Provider { get; }

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