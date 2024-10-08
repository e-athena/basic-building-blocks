namespace Athena.Infrastructure.FreeSql.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(Name = CapConstant.LockTableName)]
public class Lock
{
    /// <summary>
    /// Key
    /// </summary>
    [Key]
    [MaxLength(128)]
    [Column(Name = "key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// 实例
    /// </summary>
    [MaxLength(256)]
    [Column(Name = "instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// 最后的锁时间
    /// </summary>
    [Column(Name = "last_lock_time")]
    public DateTime LastLockTime { get; set; }
}