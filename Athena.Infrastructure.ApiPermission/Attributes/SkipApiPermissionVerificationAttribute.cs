namespace Athena.Infrastructure.ApiPermission.Attributes;

/// <summary>
/// 跳过权限验证
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class SkipApiPermissionVerificationAttribute : Attribute
{
}