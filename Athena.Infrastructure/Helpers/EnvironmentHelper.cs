namespace Athena.Infrastructure.Helpers;

/// <summary>
/// 环境帮助类
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// 是否为开发环境
    /// </summary>
    public static bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    /// <summary>
    /// 是否为测试环境
    /// </summary>
    public static bool IsTest => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

    /// <summary>
    /// 是否为生产环境
    /// </summary>
    public static bool IsProduction => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

    /// <summary>
    /// 是否为自定义环境
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsCustom(string name) => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == name;
}