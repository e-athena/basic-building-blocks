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

        if (columnPermissions == null)
        {
            return sources;
        }

        foreach (var item in sources)
        {
            // 读取属性
            var properties = item!.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                // 读取配置
                var single = columnPermissions.FirstOrDefault(p => p.ColumnKey == propertyName);
                if (single == null)
                {
                    // 跳过
                    continue;
                }

                // 读取值
                var propertyValue = property.GetValue(item);
                // 如果值为空，则跳过
                if (propertyValue == null)
                {
                    continue;
                }

                // 如果禁用，则代表没有权限，直接替换为***
                if (!single.Enabled)
                {
                    var propertyType = property.PropertyType;
                    if (propertyType == typeof(string))
                    {
                        property.SetValue(item, "***");
                    }

                    continue;
                }

                // 脱敏处理
                if (!single.IsEnableDataMask)
                {
                    continue;
                }

                // 根据长度和位置进行替换
                var value = propertyValue.ToString();
                var length = single.MaskLength;
                var maskChar = single.MaskChar;
                var maskPosition = single.MaskPosition;
                var maskLength = value!.Length - length;
                var mask = string.Empty;
                // 如果长度大于等于原始长度，则全部替换
                if (maskLength <= 0)
                {
                    for (var i = 0; i < value.Length; i++)
                    {
                        mask += maskChar;
                    }

                    property.SetValue(item, mask);
                    continue;
                }

                // 如果长度小于原始长度，则根据位置进行替换
                for (var i = 0; i < maskLength; i++)
                {
                    mask += maskChar;
                }

                // 根据位置进行替换
                switch (maskPosition)
                {
                    case MaskPosition.Front:
                        property.SetValue(item, mask + value[maskLength..]);
                        break;
                    case MaskPosition.Middle:
                        var middle = maskLength / 2;
                        property.SetValue(item, value[..middle] + mask + value[(middle + length)..]);
                        break;
                    case MaskPosition.Back:
                        property.SetValue(item, value[..maskLength] + mask);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return sources;
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilterGroup>? filterGroups
    )
    {
        return filterGroups.MakeFilterWhere<TResult>(true);
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <param name="isTrace"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilterGroup>? filterGroups, bool isTrace
    )
    {
        if (filterGroups == null || filterGroups.Count == 0)
        {
            return null;
        }

        if (isTrace)
        {
            using var activity = FreeSqlOTelActivityManager.Instance.StartActivity("生成自定义查询表达式树");
            activity?.SetTag("query.filter.json", JsonSerializer.Serialize(filterGroups));
        }

        Expression<Func<TResult, bool>>? filterGroupWhere = null;
        var parameter = Expression.Parameter(typeof(TResult), "p");
        for (var i = 0; i < filterGroups.Count; i++)
        {
            var group = filterGroups[i];

            Expression<Func<TResult, bool>>? groupLambda = null;
            foreach (var filter in group.Filters)
            {
                // 过滤掉不支持的属性
                if (!typeof(TResult).HasProperty(filter.Key))
                {
                    continue;
                }

                // 生成表达式
                var lambda = parameter.GenerateLambda<TResult>(filter);
                groupLambda = filter.XOR switch
                {
                    "or" => groupLambda.Or(lambda),
                    "and" => groupLambda.And(lambda),
                    _ => groupLambda
                };
            }

            if (i > 0)
            {
                filterGroupWhere = group.XOR switch
                {
                    "or" => filterGroupWhere.Or(groupLambda),
                    "and" => filterGroupWhere.And(groupLambda),
                    _ => groupLambda
                };
            }
            else
            {
                filterGroupWhere = groupLambda;
            }
        }

        return filterGroupWhere;
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
        return filters.MakeFilterWhere<TResult>(true);
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="isTrace"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilter>? filters, bool isTrace
    )
    {
        if (filters == null || filters.Count == 0)
        {
            return null;
        }

        if (isTrace)
        {
            using var activity = FreeSqlOTelActivityManager.Instance.StartActivity("生成自定义查询表达式树");
            activity?.SetTag("query.filter.json", JsonSerializer.Serialize(filters));
        }

        var parameter = Expression.Parameter(typeof(TResult), "p");
        Expression<Func<TResult, bool>>? filterLambda = null;
        foreach (var filter in filters)
        {
            // 过滤掉不支持的属性
            if (!typeof(TResult).HasProperty(filter.Key))
            {
                continue;
            }

            // 生成表达式
            var lambda = parameter.GenerateLambda<TResult>(filter);
            filterLambda = filter.XOR switch
            {
                "or" => filterLambda.Or(lambda),
                "and" => filterLambda.And(lambda),
                _ => filterLambda
            };
        }

        return filterLambda;
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

    // 属性缓存
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <summary>
    /// 是否存在属性
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public static bool HasProperty(this Type type, string propertyName)
    {
        // 从缓存中读取
        if (PropertyCache.TryGetValue(type, out var properties))
        {
            return properties.Any(p => p.Name == propertyName);
        }

        properties = type.GetProperties();
        // 添加到缓存
        PropertyCache.TryAdd(type, properties);

        return properties.Any(p => p.Name == propertyName);
    }
}