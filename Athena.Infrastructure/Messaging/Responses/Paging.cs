namespace Athena.Infrastructure.Messaging.Responses;

/// <summary>
/// 分页对象
/// </summary>
public class Paging<T>
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public long CurrentPage { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public long TotalPages { get; set; }

    /// <summary>
    /// 总记录数
    /// </summary>
    public long TotalItems { get; set; }

    /// <summary>
    /// 每页的记录数
    /// </summary>
    public long ItemsPerPage { get; set; }

    /// <summary>
    /// 数据集
    /// </summary>
    public List<T>? Items { get; set; }

    /// <summary>
    /// 是否为第一页
    /// </summary>
    public bool IsFirstPage => CurrentPage == 1;

    /// <summary>
    /// 是否为最后一页
    /// </summary>
    public bool IsLastPage => CurrentPage == TotalPages;

    /// <summary>
    /// 是否有值
    /// </summary>
    /// <returns></returns>
    public bool HasItems()
    {
        return Items is {Count: > 0};
    }
}