// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 
/// </summary>
public static class AthenaWebApplicationBuilderExtensions
{
    /// <summary>
    /// Runs an application and block the calling thread until host shutdown.
    /// </summary>
    /// <param name="webApplication"></param>
    /// <param name="mapActions"></param>
    /// <param name="useActions"></param>
    /// <typeparam name="TType"></typeparam>
    public static WebApplication UseAthena<TType>(this WebApplication webApplication,
        Action<WebApplication>? mapActions = null,
        Action<WebApplication>? useActions = null)
    {
        var configuration = webApplication.Services.GetService<IConfiguration>();
        webApplication.UseCustomAthena<TType>(
            useActions: (app) =>
            {
                if (configuration != null && !configuration.GetValue<bool>("Module:DbContext:Disabled"))
                {
                    app.UseCustomSqlSugarMultiTenancy();
                }

                useActions?.Invoke(app);
            },
            map =>
            {
                map.MapSyncDatabase();
                mapActions?.Invoke(map);
            });
        return webApplication;
    }

    /// <summary>
    /// Runs an application and block the calling thread until host shutdown.
    /// </summary>
    /// <param name="app"></param>
    /// <typeparam name="TType"></typeparam>
    public static void RunAthena<TType>(this WebApplication app)
    {
        app.UseAthena<TType>();
        app.Run();
    }

    /// <summary>
    /// 同步数据库表结构
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void MapSyncDatabase(this WebApplication app)
    {
        app.MapGet("/api/external/sync-database", () =>
            {
                var freeSql = AthenaProvider.Provider?.GetService<ISqlSugarClient>();
                freeSql?.SyncStructure();
                return new ApiResult<string>
                {
                    Success = true,
                    Data = "ok"
                };
            })
            .AddEndpointFilter<BasicAuthEndpointFilter>();
    }
}