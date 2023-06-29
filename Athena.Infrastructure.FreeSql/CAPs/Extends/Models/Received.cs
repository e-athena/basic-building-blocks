using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace Athena.Infrastructure.FreeSql.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(Name = CapConstant.ReceivedTableName)]
// [Index("IX_Received_ExpiresAt", nameof(ExpiresAt), false)]
public class Received
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
    public string? Version { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 400)]
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
    [Column(StringLength = 50)]
    public string StatusName { get; set; } = null!;
}