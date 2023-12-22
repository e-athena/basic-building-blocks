using System.ComponentModel.DataAnnotations;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(CapConstant.LockTableName)]
public class Lock
{
    /// <summary>
    /// Key
    /// </summary>
    [Key]
    [MaxLength(128)]
    public string Key { get; set; } = null!;

    /// <summary>
    /// 实例
    /// </summary>
    [MaxLength(256)]
    public string? Instance { get; set; }

    /// <summary>
    /// 最后的锁时间
    /// </summary>
    public DateTime LastLockTime { get; set; }
}