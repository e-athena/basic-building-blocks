using Athena.Infrastructure.Messaging.Requests;
using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.EfCore;

public static class QueryableExtensions
{
    /// <summary>
    /// 读取分页数据结果
    /// </summary>
    /// <param name="result">结果集</param>
    /// <param name="rsp">响应类</param>
    /// <param name="func"></param>
    /// <typeparam name="TEntity">实体</typeparam>
    /// <typeparam name="TView">DTO</typeparam>
    /// <returns></returns>
    public static ApiResult<Page<TView>> GetPagesResult<TEntity, TView>(this Page<TEntity> result,
        ApiResult<Page<TView>> rsp, Func<TEntity, TView> func)
    {
        if (!result.HasItems())
        {
            rsp.Message = "暂无数据";
            return rsp;
        }

        rsp.Success = true;
        rsp.Message = "读取成功";
        rsp.Data = result.ToViewPage(func);
        return rsp;
    }

    public static IQueryable<TSource> HasWhere<TSource>(this IQueryable<TSource> query, bool flag,
        Expression<Func<TSource, bool>> whExpression)
    {
        return flag ? query.Where(whExpression) : query;
    }

    public static IQueryable<TSource> HasWhere<TSource>(this IQueryable<TSource> query, object? target,
        Expression<Func<TSource, bool>> whExpression)
    {
        if (target != null && !string.IsNullOrEmpty(target.ToString()))
        {
            query = query.Where(whExpression);
        }

        return query;
    }

    public static IQueryable<TSource> HasSorter<TSource>(this IQueryable<TSource> query, string ordering)
    {
        if (!string.IsNullOrEmpty(ordering))
        {
            query = query.OrderBy(ordering);
        }

        return query;
    }

    public static IQueryable<TSource> HasWhere<TSource>(this IQueryable<TSource> query, object? target1,
        object? target2,
        Expression<Func<TSource, bool>> whExpression)
    {
        if (target1 != null && target2 != null)
        {
            query = query.Where(whExpression);
        }

        return query;
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    private static async Task<Page<T>> ToPageAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize
    )
    {
        var page = new Page<T>();
        var totalItems = await query.CountAsync();
        var totalPages = totalItems != 0
            ? totalItems % pageSize == 0 ? totalItems / pageSize : totalItems / pageSize + 1
            : 0;
        page.CurrentPage = pageIndex;
        page.ItemsPerPage = pageSize;
        page.TotalItems = totalItems;
        page.TotalPages = totalPages;
        page.Items = totalItems == 0
            ? new List<T>()
            : await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return page;
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="sorter"></param>
    /// <returns></returns>
    private static async Task<Page<T>> ToPageAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        string? sorter
    )
    {
        // 如果有排序
        if (!string.IsNullOrEmpty(sorter))
        {
            query = query.OrderBy(sorter);
        }

        return await query.ToPageAsync(pageIndex, pageSize);
    }

    /// <summary>
    ///     以特定的条件运行组合两个Expression表达式
    /// </summary>
    /// <typeparam name="T">表达式的主实体类型</typeparam>
    /// <param name="expr1">第一个Expression表达式</param>
    /// <param name="expr2">要组合的Expression表达式</param>
    /// <returns>组合后的表达式</returns>
    public static Expression<Func<T, bool>> OrAlso<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> query, GetPageRequestBase request)
    {
        var pageIndex = request.PageIndex;
        var pageSize = request.PageSize;
        return await query.ToPageAsync(pageIndex, pageSize, request.Sorter);
    }

    /// <summary>
    /// 转化Page
    /// </summary>
    /// <typeparam name="TEntity">转化前</typeparam>
    /// <typeparam name="TView">转化后</typeparam>
    /// <param name="page"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    private static Page<TView> ToViewPage<TEntity, TView>(this Page<TEntity> page,
        Func<TEntity, TView> func)
    {
        var view = new Page<TView>
        {
            CurrentPage = page.CurrentPage,
            ItemsPerPage = page.ItemsPerPage,
            TotalItems = page.TotalItems,
            TotalPages = page.TotalPages,
            Items = new List<TView>()
        };
        if (page.HasItems())
        {
            view.Items = page.Items?.Select(func).ToList();
        }

        return view;
    }
}