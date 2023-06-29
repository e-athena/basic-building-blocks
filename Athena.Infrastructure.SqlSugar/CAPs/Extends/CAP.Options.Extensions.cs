// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class CapOptionsExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static CapOptions UseSqlSugar(
        this CapOptions options
    )
    {
        options.RegisterExtension(new SqlSugarCapOptionsExtension());
        return options;
    }
}