namespace Athena.Infrastructure.Domain;

/// <summary>
/// 创建人
/// </summary>
public interface ICreator
{
    /// <summary>
    /// 创建人ID
    /// </summary>
    [MaxLength(36)]
    string? CreatedUserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    DateTime CreatedOn { get; set; }
}