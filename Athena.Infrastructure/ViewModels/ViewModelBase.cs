namespace Athena.Infrastructure.ViewModels;

/// <summary>
/// 
/// </summary>
public class ViewModelBase
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? UpdatedOn { get; set; }
}