using Athena.Infrastructure.ColumnPermissions;
using Athena.Infrastructure.ColumnPermissions.Models;

namespace Athena.Infrastructure.FreeSql;

/// <summary>
/// QueryableExtensions
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="flag"></param>
    /// <param name="whereExpression"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static ISelect<TSource> HasWhere<TSource>(this ISelect<TSource> query, bool flag,
        Expression<Func<TSource, bool>> whereExpression) where TSource : class
    {
        return flag ? query.Where(whereExpression) : query;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="target"></param>
    /// <param name="whereExpression"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static ISelect<TSource> HasWhere<TSource>(this ISelect<TSource> query, object? target,
        Expression<Func<TSource, bool>> whereExpression) where TSource : class
    {
        if (target != null && !string.IsNullOrEmpty(target.ToString()))
        {
            query = query.Where(whereExpression);
        }

        return query;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <param name="dateRange"></param>
    /// <param name="whereExpression"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static ISelect<TSource> HasWhere<TSource>(this ISelect<TSource> query, IList<DateTime>? dateRange,
        Expression<Func<TSource, bool>> whereExpression) where TSource : class
    {
        if (dateRange is {Count: 2})
        {
            query = query.Where(whereExpression);
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
    public static async Task<Page<T>> ToPageAsync<T>(this ISelect<T> query, int pageIndex, int pageSize) where T : class
    {
        return await query.ToPageAsync<T, T>(pageIndex, pageSize, null, null);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<Page<TResult>> ToPageAsync<T, TResult>(this ISelect<T> query,
        int pageIndex, int pageSize) where T : class
    {
        return await query.ToPageAsync<T, TResult>(pageIndex, pageSize, null, null);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Page<TResult>> ToPageAsync<T, TResult>(this ISelect<T> query,
        int pageIndex, int pageSize, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToPageAsync(pageIndex, pageSize, null, funcExpression, cancellationToken);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Page<T>> ToPageAsync<T>(this ISelect<T> query, GetPageRequestBase request,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToPageAsync<T, T>(request.PageIndex, request.PageSize, request.Sorter, null,
            cancellationToken);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Page<TResult>> ToPageAsync<T, TResult>(this ISelect<T> query,
        GetPageRequestBase request, CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToPageAsync<T, TResult>(request.PageIndex, request.PageSize, request.Sorter, null,
            cancellationToken);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Page<TResult>> ToPageAsync<T, TResult>(this ISelect<T> query,
        GetPageRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToPageAsync(request.PageIndex, request.PageSize, request.Sorter, funcExpression,
            cancellationToken);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="sorter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<Page<TResult>> ToPageAsync<T, TResult>(
        this ISelect<T> query,
        int pageIndex,
        int pageSize,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        if (sorter != null)
        {
            query = query.OrderBy(sorter);
        }

        query = query.Count(out var totalItems).Page(pageIndex, pageSize);

        var totalPages = totalItems != 0
            ? totalItems % pageSize == 0 ? totalItems / pageSize : totalItems / pageSize + 1
            : 0;

        var page = new Page<TResult>
        {
            Items = new List<TResult>(),
            CurrentPage = pageIndex,
            ItemsPerPage = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        page.Items = funcExpression != null
            ? await query.ToListAsync(funcExpression, cancellationToken)
            : await query.ToListAsync<TResult>(cancellationToken);

        return page;
    }

    /// <summary>
    /// 转化Pages
    /// </summary>
    /// <typeparam name="TSource">转化前</typeparam>
    /// <typeparam name="TResult">转化后</typeparam>
    /// <param name="page"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Page<TResult> ToViewPages<TSource, TResult>(this Page<TSource> page, Func<TSource, TResult> func)
    {
        var view = new Page<TResult>
        {
            CurrentPage = page.CurrentPage,
            ItemsPerPage = page.ItemsPerPage,
            TotalItems = page.TotalItems,
            TotalPages = page.TotalPages,
            Items = new List<TResult>()
        };
        if (page.HasItems())
        {
            view.Items = page.Items?.Select(func).ToList();
        }

        return view;
    }

    /// <summary>
    /// 兼容IQueryable的Select方法
    /// </summary>
    /// <param name="query"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <returns></returns>
    public static ISelect<TDto> Select<TSource, TDto>(this ISelect<TSource> query,
        Expression<Func<TSource, TDto>> selector)
    {
        // 自动转换
        return query.WithTempQuery(selector.AutomaticConverter());
    }

    #region List

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<T>> ToListAsync<T>(
        this ISelect<T> query,
        string? userId,
        GetRequestBase request,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToListAsync(request, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(T));
        }

        if (filterGroups == null)
        {
            // return await query.ToListAsync(request, cancellationToken);
            return await query
                .ToListAsync<T, T>(
                    userId,
                    sorter: request.Sorter,
                    funcExpression: null,
                    cancellationToken
                );
        }

        var lambda = filterGroups.MakeFilterWhere<T>();
        return await query
            .HasWhere(lambda, lambda!)
            .ToListAsync<T, T>(
                userId,
                sorter: request.Sorter,
                funcExpression: null,
                cancellationToken
            );
        // .ToListAsync(request, cancellationToken);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<T>> ToListAsync<T>(this ISelect<T> query, GetRequestBase request,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToListAsync<T, T>(
            request,
            null,
            cancellationToken
        );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISelect<T> query,
        string? userId, CancellationToken cancellationToken = default)
        where T : class where TResult : new()
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToListAsync<TResult>(cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        if (filterGroups == null)
        {
            return await query.ToListAsync<T, TResult>(userId, sorter: null, funcExpression: null, cancellationToken);
        }

        var extraLambda = filterGroups.MakeFilterWhere<TResult>();
        if (extraLambda == null)
        {
            return await query.ToListAsync<T, TResult>(userId, sorter: null, funcExpression: null, cancellationToken);
        }

        Expression<Func<T, TResult>> funcExpression = p => new TResult();
        return await query
            .Select(funcExpression)
            .Where(extraLambda)
            .ToListAsync<TResult, TResult>(
                userId,
                null,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISelect<T> query,
        string? userId, GetRequestBase request, CancellationToken cancellationToken = default)
        where T : class where TResult : new()
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToListAsync<T, TResult>(request, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        if (filterGroups == null)
        {
            return await query.ToListAsync<T, TResult>(
                userId,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        var extraLambda = filterGroups.MakeFilterWhere<TResult>();
        if (extraLambda == null)
        {
            return await query.ToListAsync<T, TResult>(
                userId,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        var customLambda = request.FilterGroups.MakeFilterWhere<TResult>();
        Expression<Func<T, TResult>> funcExpression = p => new TResult();
        return await query
            .Select(funcExpression)
            .Where(extraLambda)
            .WhereIf(customLambda != null, customLambda)
            .ToListAsync<TResult, TResult>(
                userId,
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISelect<T> query,
        GetRequestBase request, CancellationToken cancellationToken = default) where T : class
    {
        var lambda = request.FilterGroups.MakeFilterWhere<T>();

        return await query
            .HasWhere(lambda, lambda!)
            .ToListAsync<T, TResult>(
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISelect<T> query,
        Expression<Func<T, TResult>> funcExpression, string? userId,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToListAsync(funcExpression, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        var extraLambda = filterGroups.MakeFilterWhere<TResult>();

        return await query
            .Select(funcExpression)
            .WhereIf(extraLambda != null, extraLambda)
            .ToListAsync<TResult, TResult>(
                userId,
                null,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISelect<T> query,
        string? userId,
        GetRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        if (funcExpression == null)
        {
            return await query.ToListAsync<T, TResult>(userId, request.Sorter, null, cancellationToken);
        }

        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToListAsync<T, TResult>(request, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        var customLambda = request.FilterGroups.MakeFilterWhere<TResult>();
        var extraLambda = filterGroups.MakeFilterWhere<TResult>();

        if (customLambda == null && extraLambda == null)
        {
            return await query.ToListAsync<T, TResult>(
                userId,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        return await query
            .Select(funcExpression)
            .WhereIf(customLambda != null, customLambda)
            .WhereIf(extraLambda != null, extraLambda)
            .ToListAsync<TResult, TResult>(
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISelect<T> query,
        GetRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        if (funcExpression == null)
        {
            return await query.ToListAsync<T, TResult>(request, cancellationToken);
        }

        var lambda = request.FilterGroups.MakeFilterWhere<TResult>();
        if (lambda == null)
        {
            return await query.ToListAsync(
                request.Sorter,
                funcExpression,
                cancellationToken
            );
        }

        return await query
            .Select(funcExpression)
            .Where(lambda)
            .ToListAsync<TResult, TResult>(
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="sorter"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<List<TResult>> ToListAsync<T, TResult>(
        this ISelect<T> query,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = FreeSqlOTelActivityManager.Instance.StartActivity("数据列表查询");
        if (!string.IsNullOrEmpty(sorter))
        {
            activity?.SetTag("query.sorter", sorter);
        }

        var hasLambda = funcExpression != null;
        if (sorter != null)
        {
            query = query.OrderBy(sorter);
        }

        // 兼容组织架构数据权限查询
        query = query.InnerJoinHandler();

        using var listActivity = FreeSqlOTelActivityManager.Instance.StartActivity("读取列表数据");
        listActivity?.SetTag("query.sql.text", query.ToSql());
        var result = hasLambda
            ? await query.ToListAsync(funcExpression, cancellationToken)
            : await query.ToListAsync<TResult>(cancellationToken);

        return result;
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="sorter"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<List<TResult>> ToListAsync<T, TResult>(
        this ISelect<T> query,
        string? userId,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    )
    {
        var result = await query.ToListAsync(sorter, funcExpression, cancellationToken);
        if (string.IsNullOrEmpty(userId))
        {
            return result;
        }

        return await DataMaskHandleAsync(userId, result);
    }

    #endregion

    #region Paging

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<Paging<T>> ToPagingAsync<T>(this ISelect<T> query, int pageIndex, int pageSize)
        where T : class
    {
        return await query.ToPagingAsync<T, T>(pageIndex, pageSize, null, null);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        int pageIndex, int pageSize) where T : class
    {
        return await query.ToPagingAsync<T, TResult>(pageIndex, pageSize, null, null);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        int pageIndex, int pageSize, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToPagingAsync(pageIndex, pageSize, null, funcExpression, cancellationToken);
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<T>> ToPagingAsync<T>(
        this ISelect<T> query,
        string? userId,
        GetPagingRequestBase request,
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToPagingAsync(request, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(T));
        }

        if (filterGroups == null)
        {
            return await query.ToPagingAsync<T, T>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        var lambda = filterGroups.MakeFilterWhere<T>();
        return await query
            .HasWhere(lambda, lambda!)
            .ToPagingAsync<T, T>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<T>> ToPagingAsync<T>(this ISelect<T> query, GetPagingRequestBase request,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query.ToPagingAsync<T, T>(
            request,
            null,
            cancellationToken
        );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        string? userId, GetPagingRequestBase request, CancellationToken cancellationToken = default)
        where T : class where TResult : new()
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        if (filterGroups == null)
        {
            return await query.ToPagingAsync<T, TResult>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        var extraLambda = filterGroups.MakeFilterWhere<TResult>();
        if (extraLambda == null)
        {
            return await query.ToPagingAsync<T, TResult>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        var customLambda = request.FilterGroups.MakeFilterWhere<TResult>();
        Expression<Func<T, TResult>> funcExpression = p => new TResult();
        return await query
            .Select(funcExpression)
            .Where(extraLambda)
            .WhereIf(customLambda != null, customLambda)
            .ToPagingAsync<TResult, TResult>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        GetPagingRequestBase request, CancellationToken cancellationToken = default) where T : class
    {
        var lambda = request.FilterGroups.MakeFilterWhere<T>();

        return await query
            .HasWhere(lambda, lambda!)
            .ToPagingAsync<T, TResult>(
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        string? userId,
        GetPagingRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        if (funcExpression == null)
        {
            return await query.ToPagingAsync<T, TResult>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        if (string.IsNullOrEmpty(userId))
        {
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        var customLambda = request.FilterGroups.MakeFilterWhere<TResult>();
        var extraLambda = filterGroups.MakeFilterWhere<TResult>();

        if (customLambda == null && extraLambda == null)
        {
            return await query.ToPagingAsync(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                funcExpression,
                cancellationToken
            );
        }

        return await query
            .Select(funcExpression)
            .WhereIf(customLambda != null, customLambda)
            .WhereIf(extraLambda != null, extraLambda)
            .ToPagingAsync<TResult, TResult>(
                userId,
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        GetPagingRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        if (funcExpression == null)
        {
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        var lambda = request.FilterGroups.MakeFilterWhere<TResult>();
        if (lambda == null)
        {
            return await query.ToPagingAsync(
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                funcExpression,
                cancellationToken
            );
        }

        return await query
            .Select(funcExpression)
            .Where(lambda)
            .ToPagingAsync<TResult, TResult>(
                request.PageIndex,
                request.PageSize,
                request.Sorter,
                null,
                cancellationToken
            );
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="sorter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(
        this ISelect<T> query,
        int pageIndex,
        int pageSize,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    )
    {
        using var activity = FreeSqlOTelActivityManager.Instance.StartActivity("分页数据查询");
        activity?.SetTag("query.page.index", pageIndex.ToString());
        activity?.SetTag("query.page.size", pageSize.ToString());
        if (!string.IsNullOrEmpty(sorter))
        {
            activity?.SetTag("query.sorter", sorter);
        }

        var hasLambda = funcExpression != null;
        if (sorter != null)
        {
            query = query.OrderBy(sorter);
        }

        // 兼容组织架构数据权限查询
        query = query.InnerJoinHandler();

        // sql = query.ToSql();
        long totalItems;
        using (var countActivity = FreeSqlOTelActivityManager.Instance.StartActivity("读取总记录数"))
        {
            totalItems = await query.CountAsync(cancellationToken);
            countActivity?.SetTag("query.sql.text", query.ToSql());
            countActivity?.SetTag("result.total.count", totalItems.ToString());
        }

        activity?.SetTag("query.sql.text", query.ToSql());
        var totalPages = totalItems != 0
            ? totalItems % pageSize == 0 ? totalItems / pageSize : totalItems / pageSize + 1
            : 0;

        var page = new Paging<TResult>
        {
            Items = new List<TResult>(),
            CurrentPage = pageIndex,
            ItemsPerPage = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        if (totalItems == 0)
        {
            return page;
        }

        using (var listActivity = FreeSqlOTelActivityManager.Instance.StartActivity("读取列表数据"))
        {
            query = query.Page(pageIndex, pageSize);
            listActivity?.SetTag("query.sql.text", query.ToSql());
            page.Items = hasLambda
                ? await query.ToListAsync(funcExpression, cancellationToken)
                : await query.ToListAsync<TResult>(cancellationToken);
        }

        return page;
    }

    /// <summary>
    /// 读取列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="query"></param>
    /// <param name="sorter"></param>
    /// <param name="userId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(
        this ISelect<T> query,
        string? userId,
        int pageIndex,
        int pageSize,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    )
    {
        var result = await query.ToPagingAsync(pageIndex, pageSize, sorter, funcExpression, cancellationToken);
        if (!result.HasItems() || string.IsNullOrEmpty(userId))
        {
            return result;
        }

        // 脱敏和数据处理。
        result.Items = await DataMaskHandleAsync(userId, result.Items!);
        return result;
    }

    // 处理关联表的数据权限
    private static ISelect<T> InnerJoinHandler<T>(this ISelect<T> query)
    {
        var sql = query.ToSql();
        // 兼容组织架构数据权限查询
        if (!sql.Contains("boa.OrganizationalUnitId"))
        {
            return query;
        }

        var hasId = typeof(T).HasProperty("Id");
        if (!hasId)
        {
            throw new Exception($"类型{typeof(T).Name}没有Id属性");
        }

        // 如果T没继承EntityCore，则排除 && boa.BusinessTable == typeof(T).Name
        var isEntityCore = typeof(T).IsSubclassOf(typeof(EntityCore));
        var parameter = Expression.Parameter(typeof(T), "p");
        var parameter2 = Expression.Parameter(typeof(OrganizationalUnitAuth), "boa");
        var left = Expression.Property(parameter, "Id");
        var left2 = Expression.Property(parameter2, "BusinessId");
        var right = Expression.Property(parameter2, "BusinessTable");
        var right2 = Expression.Constant(typeof(T).Name);
        var equal = Expression.Equal(left, left2);
        var equal2 = Expression.Equal(right, right2);
        var and = Expression.AndAlso(equal, equal2);
        var lambda = Expression
            .Lambda<Func<T, OrganizationalUnitAuth, bool>>(
                isEntityCore ? and : equal,
                parameter,
                parameter2
            );

        query = query.InnerJoin(lambda).GroupBy("a.Id");

        // query = query
        //     .InnerJoin($"business_org_auths boa on a.Id=boa.BusinessId and boa.BusinessTable='{typeof(T).Name}'")
        //     .GroupBy("a.Id");

        return query;
    }

    /// <summary>
    /// 转化Paging
    /// </summary>
    /// <typeparam name="TSource">转化前</typeparam>
    /// <typeparam name="TResult">转化后</typeparam>
    /// <param name="page"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Paging<TResult> ToViewPaging<TSource, TResult>(this Paging<TSource> page, Func<TSource, TResult> func)
    {
        var view = new Paging<TResult>
        {
            CurrentPage = page.CurrentPage,
            ItemsPerPage = page.ItemsPerPage,
            TotalItems = page.TotalItems,
            TotalPages = page.TotalPages,
            Items = new List<TResult>()
        };
        if (page.HasItems())
        {
            view.Items = page.Items?.Select(func).ToList();
        }

        return view;
    }

    #endregion

    /// <summary>
    /// 脱敏处理
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sources"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static async Task<List<TResult>> DataMaskHandleAsync<TResult>(string userId, List<TResult> sources)
    {
        IList<ColumnPermission>? columnPermissions = null;
        var columnPermissionQueryService = AthenaProvider.GetService<IColumnPermissionService>();
        if (columnPermissionQueryService != null)
        {
            columnPermissions = await columnPermissionQueryService.GetAsync(userId, typeof(TResult));
        }

        return columnPermissions == null ? sources : ColumnPermissionHelper.DataMaskHandle(sources, columnPermissions);
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(this IList<QueryFilterGroup>? filterGroups)
    {
        return filterGroups.MakeFilterWhere<TResult>(true, typeof(DbFunc));
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filters"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilter>? filters
    )
    {
        return filters.MakeFilterWhere<TResult>(true, typeof(DbFunc));
    }
}

/// <summary>
///
/// </summary>
[ExpressionCall]
public static class DbFunc
{
    //必要定义 static + ThreadLocal
    private static readonly ThreadLocal<ExpressionCallContext> Context = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="that"></param>
    /// <param name="arg1"></param>
    /// <returns></returns>
    public static bool FormatSubQuery(this string that, string arg1)
    {
        var up = Context.Value;
        if (up == null)
        {
            return true;
        }

        // 重写内容
        up.Result = $"(({up.ParsedContent["arg1"]}) IN ({that}))";
        return true;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="that"></param>
    /// <param name="arg1"></param>
    /// <returns></returns>
    public static bool FormatInnerJoin(this string that, string arg1)
    {
        var up = Context.Value;
        if (up == null)
        {
            return true;
        }

        // 将that转成值，例：a,b,c -> 'a','b','c'
        var thatValue = string.Join(',', that.Split(',').Select(p => $"'{p}'"));
        // 重写内容
        up.Result = $"boa.OrganizationalUnitId IN ({thatValue})";

        return true;
    }
}