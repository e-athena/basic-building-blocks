using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Athena.Infrastructure.Expressions;

/// <summary>
/// 表达式树护类
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 缓存表达式树
    /// </summary>
    private static readonly Dictionary<string, object> ExpressionCacheDict = new();

    /// <summary>
    /// 自动转换同名的属性类型相同的属性
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <returns></returns>
    public static Expression<Func<TSource, TDto>> AutomaticConverter<TSource, TDto>(
        this Expression<Func<TSource, TDto>> expression)
    {
        var parameter = expression.Parameters.First();
        var key = $"Key_{typeof(TSource).FullName}_To_{typeof(TDto).FullName}";
        if (ExpressionCacheDict.TryGetValue(key, out var value))
        {
            return (Expression<Func<TSource, TDto>>) value;
        }

        var response = Expression.New(typeof(TDto));
        var bindings = typeof(TDto)
            .GetProperties()
            // 过滤掉NotMapped或JsonIgnore
            .Where(property =>
                property.GetCustomAttribute<NotMappedAttribute>() == null &&
                property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .Select(property =>
            {
                var sourceProperty = typeof(TSource).GetProperty(property.Name);
                if (sourceProperty == null || sourceProperty.PropertyType != property.PropertyType)
                {
                    // 如果找不到同名属性，则在传入的表达式中查找
                    var memberExpression = expression.Body as MemberInitExpression;
                    var member = memberExpression?
                        .Bindings
                        .FirstOrDefault(b => b.Member.Name == property.Name);
                    if (member is MemberAssignment memberAssignment)
                    {
                        return memberAssignment;
                    }
                }

                if (sourceProperty == null || sourceProperty.PropertyType != property.PropertyType) return null;
                {
                    var memberAssignment =
                        Expression.Bind(property, Expression.PropertyOrField(parameter, sourceProperty.Name));
                    return memberAssignment;
                }
            }).OfType<MemberBinding>();
        // 生成Lambda表达式
        var lambda = Expression.Lambda<Func<TSource, TDto>>(Expression.MemberInit(response, bindings), parameter);
        // 添加缓存
        ExpressionCacheDict[key] = lambda;
        return lambda;
    }
}