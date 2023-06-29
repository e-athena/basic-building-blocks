namespace Athena.Infrastructure.ViewModels;

/// <summary>
/// 文件
/// </summary>
public class FileViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public FileViewModel()
    {
        FileToken = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="fileType"></param>
    public FileViewModel(string fileName, string fileType)
    {
        FileName = fileName;
        FileType = fileType;
        FileToken = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 文件名
    /// </summary>
    [Required]
    public string FileName { get; set; } = null!;

    /// <summary>
    /// 文件类型
    /// </summary>
    [Required]
    public string FileType { get; set; } = null!;

    /// <summary>
    /// 文件Token
    /// </summary>
    [Required]
    public string FileToken { get; set; }
}