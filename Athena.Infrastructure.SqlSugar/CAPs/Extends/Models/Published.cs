using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(CapConstant.PublishedTableName)]
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
    [SugarColumn(Length = 20)]
    public string Version { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(Length = 200)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(Length = 200)]
    public string? Group { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(Length = -1)]
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
    [SugarColumn(Length = 40)]
    public string StatusName { get; set; } = null!;
}