

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 
/// </summary>
public static class AthenaProviderBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAthenaProvider(this IApplicationBuilder app)
    {
        AthenaProvider.Provider = app.ApplicationServices;
        AthenaProvider.DefaultLog = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger("Default");
        return app;
    }
}