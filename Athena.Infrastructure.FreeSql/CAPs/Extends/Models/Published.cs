using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace Athena.Infrastructure.FreeSql.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(Name = CapConstant.PublishedTableName)]
// [Index("IX_Published_ExpiresAt", nameof(ExpiresAt), false)]
public class Published
{
    /// <summary>
    /// 
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 20)]
    public string Version { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 200)]
    public string? Group { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = -1)]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    public int Retries { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime Added { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 40)]
    public string StatusName { get; set; } = null!;
}