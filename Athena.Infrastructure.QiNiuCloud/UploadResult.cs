namespace Athena.Infrastructure.QiNiuCloud;

/// <summary>
/// UploadResult
/// </summary>
public class UploadResult
{
    /// <summary>
    /// Bucket
    /// </summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Hash
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Size
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// MimeType
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// EndUser
    /// </summary>
    public string EndUser { get; set; } = string.Empty;

    /// <summary>
    /// Ext
    /// </summary>
    public string Ext { get; set; } = string.Empty;

    /// <summary>
    /// Error
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// 是否有错误
    /// </summary>
    /// <returns></returns>
    public bool HasError() => !string.IsNullOrEmpty(Error);
}