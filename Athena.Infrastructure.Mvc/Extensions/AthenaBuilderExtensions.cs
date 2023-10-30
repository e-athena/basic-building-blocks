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
        var type = typeof(T);
        // 读取assemblyName
        var assemblyName = type.Assembly.GetName().Name;
        // assemblyName
        var embeddedFileNamespace = $"{assemblyName}.wwwroot";
        var name = $"{embeddedFileNamespace}.index.html";
        app.MapGet(pattern, async httpContext =>
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentType = "text/html;charset=utf-8";

            await using var stream = type.Assembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                // 
                await httpContext.Response.WriteAsync("Welcome to Athena Pro", Encoding.UTF8);
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