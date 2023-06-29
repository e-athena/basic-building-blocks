namespace Athena.Infrastructure.Messaging.Requests;

/// <summary>
/// Id请求类
/// </summary>
public class IdRequest
{
    /// <summary>
    /// Id
    /// </summary>
    [Required]
    public string Id { get; set; } = null!;
}