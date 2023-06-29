namespace Athena.Infrastructure.DataPermission;

/// <summary>
/// 数据权限配置
/// </summary>
public class DataPermissionConfig
{
    /// <summary>
    /// 缓存Key格式化
    /// </summary>
    public string CacheKeyFormat { get; set; } = "user:{0}:UserPolicyQuery:{1}";

    /// <summary>
    /// 是否启用缓存
    /// </summary>
    public bool IsEnableCache { get; set; } = true;

    /// <summary>
    /// 缓存有效期/秒
    /// </summary>
    public int CacheExpireSeconds { get; set; } = 1800;
}