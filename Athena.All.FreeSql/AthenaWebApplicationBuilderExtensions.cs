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
            useActions: app =>
            {
                if (configuration != null && !configuration.GetEnvValue<bool>("Module:DbContext:Disabled"))
                {
                    app.UseCustomFreeSqlMultiTenancy();
                }

                useActions?.Invoke(app);
            },
            map =>
            {
                map.MapSyncDatabase();
                map.MapEventSelectList();
                map.MapEventTrackingList();
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
                var freeSql = AthenaProvider.Provider?.GetService<IFreeSql>();
                freeSql?.SyncStructure();

                return new ApiResult<string>
                {
                    Success = true,
                    Data = "ok"
                };
            })
            .AddEndpointFilter<BasicAuthEndpointFilter>();
    }

    /// <summary>
    /// 事件下拉列表
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void MapEventSelectList(this WebApplication app)
    {
        app.MapGet("/api/external/get-events", () =>
            {
                var keywords = AssemblyHelper
                    .GetAssemblyKeywords(app.Configuration, "Module:EventTracking:AssemblyKeywords");
                var data = EventTrackingHelper.GetEventSelectList(keywords);
                return new ApiResult<IList<SelectViewModel>>
                {
                    Success = true,
                    Data = data
                };
            })
            .AddEndpointFilter<BasicAuthEndpointFilter>();
    }

    /// <summary>
    /// 事件追踪配置列表
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static void MapEventTrackingList(this WebApplication app)
    {
        app.MapGet("/api/external/get-event-tracking-list", () =>
            {
                var keywords = AssemblyHelper
                    .GetAssemblyKeywords(app.Configuration, "Module:EventTracking:AssemblyKeywords");
                var data = EventTrackingHelper.GetEventTrackingInfos(keywords);
                return new ApiResult<IList<EventTrackingInfo>>
                {
                    Success = true,
                    Data = data
                };
            })
            .AddEndpointFilter<BasicAuthEndpointFilter>();
    }
}