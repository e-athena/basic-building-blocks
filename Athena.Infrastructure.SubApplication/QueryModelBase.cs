namespace Athena.Infrastructure.SubApplication;

/// <summary>
/// Dto基类
/// </summary>
public class QueryModelBase
{
    /// <summary>
    /// ID
    /// </summary>
    [TableColumn(Ignore = true)]
    public string? Id { get; set; }

    /// <summary>
    /// 组织架构ID
    /// </summary>
    [TableColumn(Ignore = true)]
    public string? OrganizationalUnitId { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [TableColumn(Title = "创建人", Sort = 985, HideInTable = true, Width = 90)]
    public string? CreatedUserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [TableColumn(Title = "创建时间", Sort = 987, HideInTable = true)]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [TableColumn(Title = "更新人", Sort = 988, HideInTable = true, Width = 90)]
    public string? LastUpdatedUserId { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    [TableColumn(Title = "更新时间", Sort = 990, HideInTable = true)]
    public DateTime? UpdatedOn { get; set; }
}