namespace Athena.Infrastructure.Logger.Models;

/// <summary>
/// 服务信息
/// </summary>
[Table("services")]
public class ServiceInfo
{
    /// <summary>
    /// ID
    /// </summary>
    [Key]
    public string Id { get; set; } = ObjectId.GenerateNewStringId();

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}