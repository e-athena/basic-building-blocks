namespace Athena.Infrastructure.SubApplication.Messaging.Requests;

/// <summary>
/// 更新用户自定义列
/// </summary>
public class UpdateUserCustomColumnsRequest : IRequestBase
{
    /// <summary>
    /// 方法名
    /// </summary>
    public string MethodName => "/api/SubApplication/UpdateUserCustomColumns";

    /// <summary>
    /// 应用ID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 所属模块
    /// </summary>
    public string ModuleName { get; set; } = null!;

    /// <summary>
    /// 表格列列表
    /// </summary>
    public IList<UserCustomColumnModel> Columns { get; set; } = null!;
}