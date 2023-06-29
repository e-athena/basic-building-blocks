namespace Athena.Infrastructure.ApiPermission.Attributes;

/// <summary>
/// 权限属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ApiPermissionAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ApiPermissionAuthorizeAttribute()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="policy">Policy</param>
    public ApiPermissionAuthorizeAttribute(string policy) : base(policy)
    {
    }
}