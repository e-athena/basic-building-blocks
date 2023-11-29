namespace Athena.Infrastructure.SqlSugar;

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
    public static ISugarQueryable<TSource> HasWhere<TSource>(this ISugarQueryable<TSource> query, bool flag,
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
    public static ISugarQueryable<TSource> HasWhere<TSource>(this ISugarQueryable<TSource> query, object? target,
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
    public static ISugarQueryable<TSource> HasWhere<TSource>(this ISugarQueryable<TSource> query,
        IList<DateTime>? dateRange,
        Expression<Func<TSource, bool>> whereExpression) where TSource : class
    {
        if (dateRange is {Count: 2})
        {
            query = query.Where(whereExpression);
        }

        return query;
    }

    /// <summary>
    /// 兼容IQueryable的Select方法
    /// </summary>
    /// <param name="query"></param>
    /// <param name="selector"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <returns></returns>
    public static ISugarQueryable<TDto> Select<TSource, TDto>(this ISugarQueryable<TSource> query,
        Expression<Func<TSource, TDto>> selector)
    {
        // 自动转换
        return query.SelectMergeTable(selector.AutomaticConverter());
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
        this ISugarQueryable<T> query,
        string? userId,
        GetRequestBase request,
        CancellationToken cancellationToken = default) where T : class, new()
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
            return await query
                .ToListAsync<T, T>(
                    userId,
                    sorter: request.Sorter,
                    funcExpression: null,
                    cancellationToken
                );
        }

        var lambda = MakeFilterWhere<T>(filterGroups);
        return await query
            .HasWhere(lambda, lambda!)
            .ToListAsync<T, T>(
                userId,
                sorter: request.Sorter,
                funcExpression: null,
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
    public static async Task<List<T>> ToListAsync<T>(this ISugarQueryable<T> query, GetRequestBase request,
        CancellationToken cancellationToken = default) where T : class, new()
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
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISugarQueryable<T> query,
        string? userId, CancellationToken cancellationToken = default)
        where T : class where TResult : class, new()
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.Select<TResult>().ToListAsync(cancellationToken);
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

        var extraLambda = MakeFilterWhere<TResult>(filterGroups);
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
                sorter: null,
                funcExpression: null,
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
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISugarQueryable<T> query,
        string? userId, GetRequestBase request, CancellationToken cancellationToken = default)
        where T : class, new() where TResult : class, new()
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

        var extraLambda = MakeFilterWhere<TResult>(filterGroups);
        if (extraLambda == null)
        {
            return await query.ToListAsync<T, TResult>(
                userId,
                request.Sorter,
                null,
                cancellationToken
            );
        }

        var customLambda = MakeFilterWhere<TResult>(request.FilterGroups);
        Expression<Func<T, TResult>> funcExpression = p => new TResult();
        return await query
            .Select(funcExpression)
            .Where(extraLambda)
            .WhereIF(customLambda != null, customLambda)
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
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISugarQueryable<T> query,
        GetRequestBase request, CancellationToken cancellationToken = default) where T : class, new()
    {
        var lambda = MakeFilterWhere<T>(request.FilterGroups);

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
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISugarQueryable<T> query,
        Expression<Func<T, TResult>> funcExpression, string? userId,
        CancellationToken cancellationToken = default) where T : class where TResult : class, new()
    {
        if (string.IsNullOrEmpty(userId))
        {
            return await query.Select(funcExpression).ToListAsync(cancellationToken);
        }

        IList<QueryFilterGroup>? filterGroups = null;
        var queryFilterService = AthenaProvider.GetService<IQueryFilterService>();
        if (queryFilterService != null)
        {
            filterGroups = await queryFilterService.GetAsync(userId, typeof(TResult));
        }

        var extraLambda = MakeFilterWhere<TResult>(filterGroups);

        return await query
            .Select(funcExpression)
            .WhereIF(extraLambda != null, extraLambda)
            .ToListAsync<TResult, TResult>(
                userId,
                sorter: null,
                funcExpression: null,
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
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISugarQueryable<T> query,
        string? userId,
        GetRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class, new() where TResult : class, new()
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

        var customLambda = MakeFilterWhere<TResult>(request.FilterGroups);
        var extraLambda = MakeFilterWhere<TResult>(filterGroups);

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
            .WhereIF(customLambda != null, customLambda)
            .WhereIF(extraLambda != null, extraLambda)
            .ToListAsync<TResult, TResult>(
                request.Sorter,
                funcExpression: null,
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
    public static async Task<List<TResult>> ToListAsync<T, TResult>(this ISugarQueryable<T> query,
        GetRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class, new() where TResult : class, new()
    {
        if (funcExpression == null)
        {
            return await query.ToListAsync<T, TResult>(request, cancellationToken);
        }

        var lambda = MakeFilterWhere<TResult>(request.FilterGroups);
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
                funcExpression: null,
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
        this ISugarQueryable<T> query,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    )
    {
        var hasLambda = funcExpression != null;
        if (sorter != null)
        {
            query = query.OrderBy(sorter.Replace("a.", ""));
        }

        // 兼容组织架构数据权限查询
        query = query.InnerJoinHandler();

        var result = hasLambda
            ? await query.Select(funcExpression).ToListAsync(cancellationToken)
            : await query.Select<TResult>().ToListAsync(cancellationToken);

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
        this ISugarQueryable<T> query,
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
    public static async Task<Paging<T>> ToPagingAsync<T>(this ISugarQueryable<T> query, int pageIndex, int pageSize)
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
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISugarQueryable<T> query,
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
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISugarQueryable<T> query,
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
        this ISugarQueryable<T> query,
        string? userId,
        GetPagingRequestBase request,
        CancellationToken cancellationToken = default) where T : class, new()
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

        var lambda = MakeFilterWhere<T>(filterGroups);
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
    public static async Task<Paging<T>> ToPagingAsync<T>(this ISugarQueryable<T> query, GetPagingRequestBase request,
        CancellationToken cancellationToken = default) where T : class, new()
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
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISugarQueryable<T> query,
        string? userId, GetPagingRequestBase request, CancellationToken cancellationToken = default)
        where T : class, new() where TResult : class, new()
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

        var extraLambda = MakeFilterWhere<TResult>(filterGroups);
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

        var customLambda = MakeFilterWhere<TResult>(request.FilterGroups);
        Expression<Func<T, TResult>> funcExpression = p => new TResult();
        return await query
            .Select(funcExpression)
            .Where(extraLambda)
            .WhereIF(customLambda != null, customLambda)
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
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISugarQueryable<T> query,
        GetPagingRequestBase request, CancellationToken cancellationToken = default) where T : class, new()
    {
        var lambda = MakeFilterWhere<T>(request.FilterGroups);

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
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISugarQueryable<T> query,
        string? userId,
        GetPagingRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class, new() where TResult : class, new()
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

        var customLambda = MakeFilterWhere<TResult>(request.FilterGroups);
        var extraLambda = MakeFilterWhere<TResult>(filterGroups);

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
            .WhereIF(customLambda != null, customLambda)
            .WhereIF(extraLambda != null, extraLambda)
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
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISugarQueryable<T> query,
        GetPagingRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class, new() where TResult : class, new()
    {
        if (funcExpression == null)
        {
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        var lambda = MakeFilterWhere<TResult>(request.FilterGroups);
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
        this ISugarQueryable<T> query,
        int pageIndex,
        int pageSize,
        string? sorter,
        Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default
    )
    {
        var hasLambda = funcExpression != null;
        if (sorter != null)
        {
            query = query.OrderBy(sorter.Replace("a.", ""));
        }

        // 兼容组织架构数据权限查询
        query = query.InnerJoinHandler();

        // var sql1 = query.Clone().ToSqlString();
        // var sql2 = query.Select<TResult>().Clone().ToSqlString();
        // if (hasLambda)
        // {
        //     var sql3 = query.Select(funcExpression!.AutomaticConverter()).Clone().ToSqlString();
        // }

        long totalItems = await query.CountAsync(cancellationToken);
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

        page.Items = hasLambda
            ? await query
                .Select(funcExpression!.AutomaticConverter())
                .ToPageListAsync(pageIndex, pageSize, cancellationToken)
            : await query
                .Select<TResult>()
                .ToPageListAsync(pageIndex, pageSize, cancellationToken);

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
        this ISugarQueryable<T> query,
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
    private static ISugarQueryable<T> InnerJoinHandler<T>(this ISugarQueryable<T> query)
    {
        query = query.AS(query.Context.EntityMaintenance.GetTableName(typeof(T)), "x");

        var sql = query.Clone().ToSqlString();
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

        // (p, boa) => p.Id == boa.BusinessId && boa.BusinessTable == typeof(T).Name 转成表达式树
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

        // p => p.Id转成表达式树
        var parameter3 = Expression.Parameter(typeof(T), "p");
        var left3 = Expression.Property(parameter3, "Id");
        var lambda2 = Expression.Lambda<Func<T, object>>(left3, parameter3);

        query = query.InnerJoin(lambda).GroupBy(lambda2);

        // query = query.LeftJoin<OrganizationalUnitAuth>(
        //         (p, boa) => p.Id == boa.BusinessId && boa.BusinessTable == typeof(T).Name)
        //     .GroupBy(p => p.Id);

        // query = query.AddJoinInfo(typeof(OrganizationalUnitAuth), "boa",
        //         $"x.Id=boa.BusinessId and boa.BusinessTable='{typeof(T).Name}'")
        //     .GroupBy("x.Id");

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