using Athena.Infrastructure.Files;

namespace Athena.Infrastructure.QiNiuCloud.FileObjects;

public class BucketService : IBucketService
{
    /// <summary>
    ///
    /// </summary>
    public string Provider { get; } = "QiNiu";

    public Task CreateAsync(string bucketName)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string bucketName)
    {
        throw new NotImplementedException();
    }
}