using System.Linq.Expressions;

namespace Athena.Infrastructure.QueryFilters;

/// <summary>
/// 表达式树扩展类
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// 生成lambda表达式
    /// </summary>
    /// <param name="parameterExpression"></param>
    /// <param name="filter"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResponse, bool>> GenerateLambda<TResponse>(
        this ParameterExpression parameterExpression,
        QueryFilter filter
    )
    {
        var expression = parameterExpression.GenerateExpression<TResponse>(filter);
        return Expression.Lambda<Func<TResponse, bool>>(expression, parameterExpression);
    }

    /// <summary>
    /// 生成表达式树
    /// </summary>
    /// <param name="param"></param>
    /// <param name="filter"></param>
    /// <param name="typeExtendFunc"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public static Expression GenerateExpression<TResponse>(this ParameterExpression param, QueryFilter filter, Type typeExtendFunc)
    {
        if (string.IsNullOrEmpty(filter.ExtendFuncMethodName))
        {
            return param.GenerateExpression<TResponse>(filter);
        }
        var left = Expression.Constant(filter.Value);
        var right = Expression.Property(param, filter.Key);
        var method = typeExtendFunc.GetMethod(filter.ExtendFuncMethodName, new[] { typeof(string), typeof(string) });
        return Expression.Call(null, method!, left, right);
    }

    /// <summary>
    /// 生成表达式树
    /// </summary>
    public static Expression GenerateExpression<TResponse>(this ParameterExpression param, QueryFilter filter)
    {
        var property = typeof(TResponse).GetProperty(filter.Key);

        if (property == null)
        {
            return default!;
        }

        // 组装左边
        Expression left = Expression.Property(param, property);
        // 组装右边
        Expression right = Expression.Constant(filter.Value);

        var isCommonRight = filter.Operator is ">" or "<" or "==" or "!=" or ">=" or "<=";

        // 如果是通用查询
        if (isCommonRight)
        {
            if (property.PropertyType == typeof(int))
            {
                right = Expression.Constant(int.Parse(filter.Value));
            }
            else if (property.PropertyType == typeof(int?))
            {
                left = Expression.Property(left, "Value");
                right = Expression.Constant(int.Parse(filter.Value) as int?);
            }
            else if (property.PropertyType == typeof(decimal))
            {
                right = Expression.Constant(decimal.Parse(filter.Value));
            }
            else if (property.PropertyType == typeof(decimal?))
            {
                left = Expression.Property(left, "Value");
                right = Expression.Constant(decimal.Parse(filter.Value) as decimal?);
            }
            else if (property.PropertyType == typeof(double))
            {
                right = Expression.Constant(double.Parse(filter.Value));
            }
            else if (property.PropertyType == typeof(double?))
            {
                left = Expression.Property(left, "Value");
                right = Expression.Constant(double.Parse(filter.Value) as double?);
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                right = Expression.Constant(DateTime.Parse(filter.Value));
            }
            else if (property.PropertyType == typeof(DateTime?))
            {
                left = Expression.Property(left, "Value");
                right = Expression.Constant(DateTime.Parse(filter.Value) as DateTime?);
            }
            else if (property.PropertyType == typeof(string))
            {
                right = Expression.Constant(filter.Value);
            }
            else if (property.PropertyType == typeof(bool))
            {
                right = Expression.Constant(filter.Value.Equals("1"));
            }
            else if (property.PropertyType == typeof(bool?))
            {
                left = Expression.Property(left, "Value");
                right = Expression.Constant(filter.Value.Equals("1") as bool?);
            }
            else if (property.PropertyType == typeof(Guid))
            {
                right = Expression.Constant(Guid.Parse(filter.Value));
            }
            else if (property.PropertyType == typeof(Guid?))
            {
                left = Expression.Property(left, "Value");
                right = Expression.Constant(Guid.Parse(filter.Value) as Guid?);
            }
            else if (property.PropertyType.IsEnum || Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true)
            {
                var pType = property.PropertyType.IsEnum
                    ? property.PropertyType
                    : Nullable.GetUnderlyingType(property.PropertyType)!;
                right = Expression.Constant(Enum.Parse(pType, filter.Value));
            }
            else
            {
                throw new Exception("暂不能解析该Key的类型");
            }
        }

        Expression expression;
        switch (filter.Operator)
        {
            case "<=":
                expression = Expression.LessThanOrEqual(left, right);
                break;

            case "<":
                expression = Expression.LessThan(left, right);
                break;

            case ">":
                expression = Expression.GreaterThan(left, right);
                break;
            case ">=":
                expression = Expression.GreaterThanOrEqual(left, right);
                break;
            case "!=":
                expression = Expression.NotEqual(left, right);
                break;
            case "contains":
                var method0 = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                expression = Expression.Call(left, method0!, Expression.Constant(filter.Value));
                break;
            case "in":
                if (property.PropertyType.IsEnum)
                {
                    Expression? enumExpression = null;
                    foreach (var val1 in filter.Value.Split(','))
                    {
                        var enumRightValue = Expression.Constant(Enum.Parse(property.PropertyType, val1));
                        var rightExpression = Expression.Equal(left, enumRightValue);
                        enumExpression = enumExpression == null
                            ? rightExpression
                            : enumExpression.OrElse(rightExpression);
                    }

                    expression = enumExpression!;
                    break;
                }

                var instance = Expression.Constant(filter.Value.Split(',').ToList());
                var method1 = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
                expression = Expression.Call(instance, method1!, left);
                break;
            case "not in":
                // 数组
                var listExpression = Expression.Constant(filter.Value.Split(',').ToList());
                // Contains语句
                var method2 = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) });
                expression = Expression.Not(Expression.Call(listExpression, method2!, left));
                break;
            //交集，使用交集时左值必须时固定的值
            case "intersect": //交集
                if (property != null)
                {
                    throw new Exception("交集模式下，表达式左边不能为变量，请调整数据规则，如:c=>\"A,B,C\" intersect \"B,D\"");
                }

                var rightValue = filter.Value.Split(',').ToList();
                var leftValue = filter.Key.Split(',').ToList();
                var val = rightValue.Intersect(leftValue);

                expression = Expression.Constant(val.Any());
                break;
            // 介于
            case "between":
                if (property.PropertyType != typeof(DateTime) && property.PropertyType != typeof(DateTime?))
                {
                    throw new Exception("[Between]只能用于日期时间字段");
                }

                var values = filter.Value.Split(',');
                if (values.Length != 2)
                {
                    throw new Exception("[Between]值错误，只能接受两个时间，用逗号分割");
                }

                var startTime = Expression.Constant(DateTime.Parse(values[0]));
                var endTime = Expression.Constant(DateTime.Parse(values[1]).AddDays(1));
                if (property.PropertyType == typeof(DateTime?))
                {
                    var startExpression = Expression.GreaterThanOrEqual(Expression.Property(left, "Value"), startTime);
                    var endExpression = Expression.LessThan(Expression.Property(left, "Value"), endTime);
                    expression = startExpression.AndAlso(endExpression);
                }
                else
                {
                    var startExpression = Expression.GreaterThanOrEqual(left, startTime);
                    var endExpression = Expression.LessThan(left, endTime);
                    expression = startExpression.AndAlso(endExpression);
                }

                break;
            default:
                expression = Expression.Equal(left, right);
                break;
        }

        return expression;
    }

    /// <summary>
    /// OrElse
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Expression OrElse(this Expression left, Expression right)
    {
        return Expression.OrElse(left, right);
    }

    /// <summary>
    /// AndAlso
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Expression AndAlso(this Expression left, Expression right)
    {
        return Expression.AndAlso(left, right);
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <param name="typeExtendFunc"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilterGroup>? filterGroups, Type typeExtendFunc
    )
    {
        return filterGroups.MakeFilterWhere<TResult>(true, (param, filter) => param.GenerateExpression<TResult>(filter, typeExtendFunc));
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
    /// <param name="typeExtendFunc"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilterGroup>? filterGroups,
        bool isTrace,
        Type typeExtendFunc
    )
    {
        return filterGroups.MakeFilterWhere<TResult>(isTrace, (param, filter) => param.GenerateExpression<TResult>(filter, typeExtendFunc));
    }
    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filterGroups"></param>
    /// <param name="isTrace"></param>
    /// <param name="generateExpressionFunc"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilterGroup>? filterGroups,
        bool isTrace,
        Func<ParameterExpression, QueryFilter, Expression>? generateExpressionFunc = null
    )
    {
        if (filterGroups == null || filterGroups.Count == 0)
        {
            return null;
        }

        if (isTrace)
        {
            Activity.Current?.SetTag("query.filterGroups.json", JsonSerializer.Serialize(filterGroups));
        }

        Expression? filterGroupExpression = null;
        var parameterExpression = Expression.Parameter(typeof(TResult), "p");
        for (var i = 0; i < filterGroups.Count; i++)
        {
            var group = filterGroups[i];

            Expression? groupExpression = null;
            foreach (var filter in group.Filters)
            {
                // 过滤掉不支持的属性
                if (!typeof(TResult).HasProperty(filter.Key))
                {
                    continue;
                }

                Expression expression;
                if (generateExpressionFunc == null)
                {
                    // 生成表达式
                    expression = parameterExpression.GenerateExpression<TResult>(filter);
                }
                else
                {
                    expression = generateExpressionFunc(parameterExpression, filter);
                }
                if (groupExpression == null)
                {
                    groupExpression = expression;
                    continue;
                }
                switch (filter.XOR)
                {
                    case "or":
                        groupExpression = groupExpression.OrElse(expression);
                        break;
                    case "and":
                        groupExpression = groupExpression.AndAlso(expression);
                        break;
                }
            }

            if (i > 0)
            {
                if (groupExpression == null)
                {
                    continue;

                }
                if (filterGroupExpression == null)
                {
                    filterGroupExpression = groupExpression;
                    continue;
                }
                filterGroupExpression = group.XOR switch
                {
                    "or" => filterGroupExpression.OrElse(groupExpression),
                    "and" => filterGroupExpression.AndAlso(groupExpression),
                    _ => groupExpression
                };
            }
            else
            {
                filterGroupExpression = groupExpression;
            }
        }

        if (filterGroupExpression == null)
        {
            return null;
        }

        return Expression.Lambda<Func<TResult, bool>>(filterGroupExpression, parameterExpression);
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
    /// <param name="typeExtendFunc"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilter>? filters, Type typeExtendFunc
    )
    {
        return filters.MakeFilterWhere<TResult>(true, (param, filter) => param.GenerateExpression<TResult>(filter, typeExtendFunc));
    }
    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="isTrace"></param>
    /// <param name="typeExtendFunc"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilter>? filters, bool isTrace, Type typeExtendFunc
    )
    {
        return filters.MakeFilterWhere<TResult>(isTrace, (param, filter) => param.GenerateExpression<TResult>(filter, typeExtendFunc));
    }

    /// <summary>
    /// 生成自定义查询表达式树
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="isTrace"></param>
    /// <param name="generateExpressionFunc"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TResult, bool>>? MakeFilterWhere<TResult>(
        this IList<QueryFilter>? filters, bool isTrace,
        Func<ParameterExpression, QueryFilter, Expression>? generateExpressionFunc = null
    )
    {
        if (filters == null || filters.Count == 0)
        {
            return null;
        }

        if (isTrace)
        {
            Activity.Current?.SetTag("query.filters.json", JsonSerializer.Serialize(filters));
        }

        var parameterExpression = Expression.Parameter(typeof(TResult), "p");
        Expression? filterExpression = null;
        foreach (var filter in filters)
        {
            // 过滤掉不支持的属性
            if (!typeof(TResult).HasProperty(filter.Key))
            {
                continue;
            }

            Expression expression;
            if (generateExpressionFunc == null)
            {
                // 生成表达式
                expression = parameterExpression.GenerateExpression<TResult>(filter);
            }
            else
            {
                expression = generateExpressionFunc(parameterExpression, filter);
            }
            if (filterExpression == null)
            {
                filterExpression = expression;
                continue;
            }
            filterExpression = filter.XOR switch
            {
                "or" => filterExpression.OrElse(expression),
                "and" => filterExpression.AndAlso(expression),
                _ => filterExpression
            };
        }

        if (filterExpression == null)
        {
            return null;
        }

        return Expression.Lambda<Func<TResult, bool>>(filterExpression, parameterExpression);
    }
}