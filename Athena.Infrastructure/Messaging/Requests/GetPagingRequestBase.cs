namespace Athena.Infrastructure.Messaging.Requests;

/// <summary>
/// 读取分页列表请求基类
///     所有读取分页列表的请求类需要继续此基类
/// </summary>
public class GetPagingRequestBase : GetRequestBase
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">页数据量</param>
    public GetPagingRequestBase(int pageIndex = 1, int pageSize = 20)
    {
        PageIndex = pageIndex <= 0 ? 1 : pageIndex;
        PageSize = pageSize <= 0 ? 20 : pageSize;
    }

    // 页码
    private int _pageIndex;

    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex
    {
        get
        {
            if (_pageIndex == 1 && Current > 1)
            {
                return Current;
            }

            return _pageIndex;
        }
        set => _pageIndex = value;
    }

    /// <summary>
    /// 当前页
    /// <remarks>用于兼容处理，PageIndex比Current优先级高</remarks>
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 每页的记录数
    /// </summary>
    public int PageSize { get; set; }
}