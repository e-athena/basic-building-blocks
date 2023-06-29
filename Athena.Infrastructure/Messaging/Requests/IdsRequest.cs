namespace Athena.Infrastructure.Messaging.Requests;

/// <summary>
/// Ids请求类
/// </summary>
public class IdsRequest
{
    /// <summary>
    /// Ids
    /// </summary>
    [Required]
    public IList<string> Ids { get; set; } = null!;
}