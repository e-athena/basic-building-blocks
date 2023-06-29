namespace Athena.Infrastructure.Domain;

/// <summary>
/// 更新
/// </summary>
public interface IUpdater
{
    /// <summary>
    /// 更新人
    /// </summary>
    [MaxLength(36)]
    string? LastUpdatedUserId { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    DateTime? UpdatedOn { get; set; }
}