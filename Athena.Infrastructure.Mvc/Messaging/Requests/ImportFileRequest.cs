namespace Athena.Infrastructure.Mvc.Messaging.Requests;

/// <summary>
/// 文件导入模型
/// </summary>
public class ImportFileRequest
{
    /// <summary>
    /// 文件流
    /// </summary>
    [Required]
    public IFormFile File { get; set; } = null!;
}