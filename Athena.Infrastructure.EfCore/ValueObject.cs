namespace Athena.Infrastructure.EfCore;

/// <summary>
/// 值对象需要继承的类
/// </summary>
public class ValueObject
{
    /// <summary>
    /// ID自增
    /// </summary>
    [Key]
    [Required]
    public long Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}