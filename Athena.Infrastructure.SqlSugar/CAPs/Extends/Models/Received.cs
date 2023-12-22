using System.ComponentModel.DataAnnotations;

namespace Athena.Infrastructure.SqlSugar.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(CapConstant.ReceivedTableName)]
[Index(nameof(ExpiresAt), Name = "IX_Received_ExpiresAt", IsUnique = false)]
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
    [SugarColumn(Length = 20)]
    public string? Version { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(Length = 400)]
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
    [SugarColumn(Length = 50)]
    public string StatusName { get; set; } = null!;
}