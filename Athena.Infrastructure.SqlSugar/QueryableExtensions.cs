using System.Reflection;
using Athena.Infrastructure.Providers;

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
            return await query.ToPagingAsync(request, cancellationToken);
        }

        var lambda = MakeFilterWhere<T>(filterGroups);
        return await query
            .HasWhere(lambda, lambda!)
            .ToPagingAsync(request, cancellationToken);
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
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        var extraLambda = MakeFilterWhere<TResult>(filterGroups);
        if (extraLambda == null)
        {
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        var customLambda = MakeFilterWhere<TResult>(request.FilterGroups);
        Expression<Func<T, TResult>> funcExpression = p => new TResult();
        return await query
            .Select(funcExpression)
            .Where(extraLambda)
            .WhereIF(customLambda != null, customLambda)
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
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
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
                request,
                funcExpression,
                cancellationToken
            );
        }

        return await query
            .Select(funcExpression)
            .WhereIF(customLambda != null, customLambda)
            .WhereIF(extraLambda != null, extraLambda)
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
            ? await query.Select(funcExpression!.AutomaticConverter()).ToListAsync(cancellationToken)
            : await query.Select<TResult>().ToListAsync(cancellationToken);

        return page;
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        IList<QueryFilterGroup>? filterGroups
    ) where TResult : class, new()
    {
        if (filterGroups == null || filterGroups.Count == 0)
        {
            return null;
        }

        var filterGroupWhere = Expressionable.Create<TResult>();
        var parameter = Expression.Parameter(typeof(TResult), "p");
        for (var i = 0; i < filterGroups.Count; i++)
        {
            var group = filterGroups[i];

            var groupLambda = Expressionable.Create<TResult>();
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
                    "or" => groupLambda.Or(lambda), //groupLambda.Or(lambda),
                    "and" => groupLambda.And(lambda),
                    _ => groupLambda
                };
            }

            if (i > 0)
            {
                filterGroupWhere = group.XOR switch
                {
                    "or" => filterGroupWhere.Or(groupLambda.ToExpression()),
                    "and" => filterGroupWhere.And(groupLambda.ToExpression()),
                    _ => groupLambda
                };
            }
            else
            {
                filterGroupWhere = groupLambda;
            }
        }

        return filterGroupWhere.ToExpression();
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filters"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        IList<QueryFilter>? filters
    ) where TResult : class, new()
    {
        if (filters == null || filters.Count == 0)
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(TResult), "p");
        var filterLambda = Expressionable.Create<TResult>();
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

        return filterLambda.ToExpression();
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