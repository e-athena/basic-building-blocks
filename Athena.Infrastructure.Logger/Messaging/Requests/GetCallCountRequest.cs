namespace Athena.Infrastructure.Logger.Messaging.Requests;

/// <summary>
/// 读取调用次数请求类
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class GetCallCountRequest : RequestBase
{
    /// <summary>
    /// 路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// 别名
    /// </summary>
    public string? AliasName { get; set; }
}