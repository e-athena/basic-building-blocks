using Athena.Infrastructure.Files;

namespace Athena.Infrastructure.QiNiuCloud.FileObjects;

/// <summary>
///
/// </summary>
public class BucketService : IBucketService
{
    /// <summary>
    ///
    /// </summary>
    public string Provider { get; } = "QiNiu";

    /// <summary>
    ///
    /// </summary>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task CreateAsync(string bucketName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bucketName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task DeleteAsync(string bucketName)
    {
        throw new NotImplementedException();
    }
}