// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 
/// </summary>
public static class AthenaBuilderExtensions
{
    /// <summary>Enables static file serving with the given options</summary>
    /// <param name="app"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomStaticFiles(
        this IApplicationBuilder app,
        StaticFileOptions? options = null)
    {
        return app.UseStaticFiles(options ?? new StaticFileOptions
        {
            OnPrepareResponse = c =>
            {
                // 
                c.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
        });
    }

    /// <summary>
    /// 前端路由
    /// </summary>
    /// <param name="app"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static WebApplication MapSpaFront<T>(this WebApplication app, string pattern = "/")
    {
        var config = app.Services.GetRequiredService<IConfiguration>();
        var staticFileDirectory = config.GetEnvValue<string>("Module:Web:StaticFileDirectory") ?? "wwwroot";
        var indexFileName = config.GetEnvValue<string>("Module:Web:IndexFileName") ?? "index.html";
        var defaultResponse = config.GetEnvValue<string>("Module:Web:DefaultResponse") ?? "Welcome to Athena Pro";
        var indexRoute = config.GetEnvValue<string>("Module:Web:IndexRoute") ?? "/";
        if (pattern != "/")
        {
            indexRoute = pattern;
        }

        var type = typeof(T);
        // 读取assemblyName
        var assemblyName = type.Assembly.GetName().Name;
        // assemblyName
        var embeddedFileNamespace = $"{assemblyName}.{staticFileDirectory}.";
        var name = $"{embeddedFileNamespace}{indexFileName}";
        app.MapGet(indexRoute, async httpContext =>
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentType = "text/html;charset=utf-8";

            await using var stream = type.Assembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                //
                await httpContext.Response.WriteAsync(defaultResponse, Encoding.UTF8);
                // throw new InvalidOperationException();
                return;
            }

            using var sr = new StreamReader(stream);
            var htmlBuilder = new StringBuilder(await sr.ReadToEndAsync());
            await httpContext.Response.WriteAsync(htmlBuilder.ToString(), Encoding.UTF8);
        });
        return app;
    }

    /// <summary>
    /// 健康检查
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static WebApplication MapHealth(this WebApplication app)
    {
        app.MapGet("/health", async context =>
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("ok");
        });
        return app;
    }
}