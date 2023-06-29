using System.Linq.Expressions;
using Athena.Infrastructure.Expressions;
using Athena.Infrastructure.Messaging.Requests;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.QueryFilters;

namespace Athena.Infrastructure.Logger.FreeSql;

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
        return query.WithTempQuery(selector.AutomaticConverter());
    }

    // 自动转换

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
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<T>> ToPagingAsync<T>(this ISelect<T> query, GetPagingRequestBase request,
        CancellationToken cancellationToken = default) where T : class
    {
        return await query
            .ToPagingAsync<T, T>(
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        GetPagingRequestBase request, CancellationToken cancellationToken = default) where T : class
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
    /// <param name="request"></param>
    /// <param name="funcExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paging<TResult>> ToPagingAsync<T, TResult>(this ISelect<T> query,
        GetPagingRequestBase request, Expression<Func<T, TResult>>? funcExpression,
        CancellationToken cancellationToken = default) where T : class
    {
        if (funcExpression != null)
        {
            var lambda = MakeFilterWhere<TResult>(request.FilterGroups);
            if (lambda != null)
            {
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
        }
        else
        {
            return await query.ToPagingAsync<T, TResult>(request, cancellationToken);
        }

        return await query.ToPagingAsync(
            request.PageIndex,
            request.PageSize,
            request.Sorter,
            funcExpression,
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
        var hasLambda = funcExpression != null;
        if (sorter != null)
        {
            query = query.OrderBy(sorter);
        }

        query = query.Count(out var totalItems).Page(pageIndex, pageSize);

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
            ? await query.ToListAsync(funcExpression, cancellationToken)
            : await query.ToListAsync<TResult>(cancellationToken);

        return page;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    private static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        IList<QueryFilterGroup>? filterGroups
    )
    {
        if (filterGroups == null || filterGroups.Count == 0)
        {
            return null;
        }

        Expression<Func<TResult, bool>>? filterGroupWhere = null;
        var parameter = Expression.Parameter(typeof(TResult), "p");
        for (var i = 0; i < filterGroups.Count; i++)
        {
            var group = filterGroups[i];

            Expression<Func<TResult, bool>>? groupLambda = null;
            foreach (var filter in group.Filters)
            {
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
}