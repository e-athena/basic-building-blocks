namespace Athena.Infrastructure.FreeSql.CAPs.Extends.Models;

/// <summary>
/// 
/// </summary>
[Table(Name = CapConstant.PublishedTableName)]
[DataAnnotations.Schema.Index("expires_at", Name = "IX_Published_ExpiresAt", IsUnique = false)]
public class Published
{
    /// <summary>
    /// 
    /// </summary>
    [Key]
    [Column(Name = "id")]
    public long Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 20, Name = "version")]
    public string Version { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 200, Name = "name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 200, Name = "group")]
    public string? Group { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = -1, Name = "content")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    [Column(Name = "retries")]
    public int Retries { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(Name = "added")]
    public DateTime Added { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(Name = "expires_at")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Column(StringLength = 40, Name = "status_name")]
    public string StatusName { get; set; } = null!;
}