using Athena.Infrastructure.Logger;

namespace Athena.Infrastructure.Mvc.Middlewares.AuditLogs;

/// <summary>
/// 
/// </summary>
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    public AuditLogMiddleware(RequestDelegate next)
    {
        _next = next;
        _stopwatch = new Stopwatch();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public async Task Invoke(HttpContext context)
    {
        // 默认get为读取数据
        var isQuery = context.Request.Path.ToString().ToLower().Contains("get");
        // 不保存审计日志
        if (isQuery || !context.Request.Path.HasValue || !context.Request.Path.Value.ToLower().StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        _stopwatch.Restart();
        var startTime = DateTime.Now;
        var request = context.Request;
        var reqHeaders = request
            .Headers
            .ToDictionary(x => x.Key,
                v => string.Join(";", v.Value.ToList())
            );
        var reqBody = "";
        switch (request.Method.ToLower())
        {
            case "post":
            case "put":
            {
                context.Request.EnableBuffering();
                var reqStream = new StreamReader(context.Request.Body);
                reqBody = await reqStream.ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                break;
            }
            case "get":
            case "delete":
                reqBody = (request.QueryString.HasValue ? request.QueryString.Value : "")!;
                break;
        }

        reqBody = reqBody.Replace("\n", "").Replace("\t", "");
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        await _next(context);
        var respBody = await GetResponse(context.Response);
        respBody = respBody.Replace("\n", "").Replace("\t", "");

        await responseBody.CopyToAsync(originalBodyStream);

        var endTime = DateTime.Now;

        context.Response.OnCompleted(() =>
        {
            _stopwatch.Stop();
            var configuration = context.RequestServices.GetService<IConfiguration>();
            var serviceName = configuration?.GetSection("ServiceName").Value ?? "CommonService";
            if (!bool.TryParse(configuration?.GetSection("EnableAuditLog").Value, out var enabledAuditLog))
            {
                // 默认启用
                enabledAuditLog = true;
            }

            // 检查环境变量是否启用审计日志
            var enabledAuditLogEnv = Environment.GetEnvironmentVariable("ENABLE_AUDIT_LOG");
            if (enabledAuditLogEnv != null)
            {
                enabledAuditLog = bool.TryParse(enabledAuditLogEnv, out enabledAuditLog) && enabledAuditLog;
            }

            // 不启用
            if (!enabledAuditLog)
            {
                return Task.CompletedTask;
            }

            // 获取日志服务
            var loggerService = context.RequestServices.GetService<ILoggerService>();
            if (loggerService == null)
            {
                var loggerFactory =
                    context.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                loggerService = new DefaultLoggerService(loggerFactory);
            }

            loggerService.Write(new Log
            {
                ServiceName = serviceName,
                TraceId = Activity.Current != null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier,
                ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
                StartTime = startTime,
                EndTime = endTime,
                LogLevel = context.Response.StatusCode == 200 ? Logger.LogLevel.Information : Logger.LogLevel.Error,
                Route = request.Path,
                HttpMethod = request.Method,
                RequestBody = reqBody,
                ResponseBody = respBody,
                RawData = JsonSerializer.Serialize(reqHeaders),
                StatusCode = context.Response.StatusCode,
                UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                UserName = context.User.FindFirst(ClaimTypes.Name)?.Value,
                UserAgent = request.Headers["User-Agent"],
                IpAddress = request.Headers["X-Real-IP"],
                AliasName = "AuditLogMiddleware"
            });
            return Task.CompletedTask;
        });
    }

    private static async Task<string> GetResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return text;
    }
}