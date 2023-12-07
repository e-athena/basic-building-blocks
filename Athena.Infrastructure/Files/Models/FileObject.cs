namespace Athena.Infrastructure.Files.Models;

/// <summary>
///
/// </summary>
public class FileObject
{
    /// <summary>
    /// 所属桶
    /// </summary>
    public string BucketName { get; set; } = null!;

    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 文件大小/字节
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 文件大小/human
    /// </summary>
    public string SizeHuman
    {
        get
        {
            // 根据Size计算，包含TB、GB、MB、KB、B,保留两位小数
            var size = Size;
            var units = new[] { "B", "KK", "MB", "GB", "TB" };
            var unitIndex = 0;
            while (size >= 1024)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:F2}{units[unitIndex]}";
        }
    }

    /// <summary>
    /// 文件类型
    /// </summary>
    public string MimeType { get; set; } = null!;

    /// <summary>
    /// 目录
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// 存储类型
    /// </summary>
    public int StorageClass { get; set; }

    /// <summary>
    /// MD5
    /// </summary>
    public string Md5 { get; set; } = null!;

    /// <summary>
    /// 上传时间
    /// </summary>
    public long PutTime { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Hash
    /// </summary>
    public string Hash { get; set; } = null!;

    /// <summary>
    /// 扩展属性
    /// </summary>
    public string? Extend { get; set; }
}