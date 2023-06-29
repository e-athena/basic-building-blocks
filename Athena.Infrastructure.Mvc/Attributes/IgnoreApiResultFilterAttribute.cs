namespace Athena.Infrastructure.Mvc.Attributes;

/// <summary>
/// 忽略ApiResult过滤器，正常返回数据
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IgnoreApiResultFilterAttribute : Attribute
{
}