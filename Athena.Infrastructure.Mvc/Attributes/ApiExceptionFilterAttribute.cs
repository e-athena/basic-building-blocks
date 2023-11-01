namespace Athena.Infrastructure.Mvc.Attributes;

/// <summary>
/// API异常统一处理过滤器
/// 系统级别异常 500 应用级别异常-1
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    /// <summary>
    /// ILogger
    /// </summary>
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;

    /// <summary>
    /// 主机环境变量
    /// </summary>
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="environment"></param>
    /// <param name="loggerFactory"></param>
    public ApiExceptionFilterAttribute(IWebHostEnvironment environment, ILoggerFactory loggerFactory)
    {
        _environment = environment;
        _logger = loggerFactory.CreateLogger<ApiExceptionFilterAttribute>();
    }

    /// <summary>
    /// 异常处理
    /// </summary>
    /// <param name="context"></param>
    public override async Task OnExceptionAsync(ExceptionContext context)
    {
        var ex = context.Exception;
        var traceId = Activity.Current != null
            ? Activity.Current.TraceId.ToString()
            : context.HttpContext.TraceIdentifier;
        // 应用程序业务级异常
        if (ex is FriendlyException coreException)
        {
            if (string.IsNullOrEmpty(coreException.MoreMessage))
            {
                context.Result = new ObjectResult(new
                {
                    Success = false,
                    coreException.Message,
                    coreException.StatusCode,
                    TraceId = traceId
                });
            }
            else
            {
                context.Result = new ObjectResult(new
                {
                    Success = false,
                    coreException.Message,
                    coreException.StatusCode,
                    coreException.MoreMessage,
                    TraceId = traceId
                });
            }

            // 如果是HttpStatusCode，则设置响应状态码
            var codes = Enum.GetValues<HttpStatusCode>();
            if (codes.Any(code => code.GetHashCode() == coreException.StatusCode))
            {
                context.HttpContext.Response.StatusCode = coreException.StatusCode;
            }

            await base.OnExceptionAsync(context);
            return;
        }

        // 如果是Athena.Infrastructure.FluentValidation验证异常
        if (ex.Source == "Athena.Infrastructure.FluentValidation")
        {
            // 读取ValidationResultModel值
            var validationResultModelString =
                ex.GetType().GetProperty("ValidationResultModel")?.GetValue(ex)?.ToString();
            ValidationResult? validationResult = null;
            // 反序列化ValidationResult对象
            if (validationResultModelString != null)
            {
                validationResult = JsonSerializer.Deserialize<ValidationResult>(validationResultModelString);
            }

            // validationResult
            context.Result = new ObjectResult(new CustomBadRequestResult
            {
                Success = false,
                Message = validationResult?.Message ?? "表单验证失败",
                StatusCode = validationResult?.StatusCode ?? 400,
                TraceId = traceId,
                Errors = validationResult?.Errors.Select(p => new Dictionary<string, string[]>
                {
                    {
                        p.Field ?? string.Empty,
                        new[] {p.Message}
                    }
                }).ToList()
            });
            context.HttpContext.Response.StatusCode = validationResult?.StatusCode ?? 400;
            await base.OnExceptionAsync(context);
            return;
        }

        // 如果是租户环境下
        if (TenantHelper.IsTenantEnvironment(context.HttpContext))
        {
            // 如果找不到表名，则代表租户未初始化
            if (ex.Message.Contains("no such table"))
            {
                context.Result = new ObjectResult(new
                {
                    Success = false,
                    Message = "租户未初始化，请联系管理员",
                    StatusCode = 200,
                    TraceId = traceId
                });
                context.HttpContext.Response.StatusCode = 200;
                await base.OnExceptionAsync(context);
                return;
            }
        }

        // 获取日志服务
        if (context.HttpContext.RequestServices.GetService(typeof(ILoggerService)) is not ILoggerService loggerService)
        {
            loggerService = new DefaultLoggerService(_logger);
        }

        var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        var serviceName = configuration?.GetSection("ServiceName").Value ?? "CommonService";

        var request = context.HttpContext.Request;
        // 写日志
        await loggerService.WriteAsync(new Log
        {
            ServiceName = serviceName,
            TraceId = Activity.Current != null
                ? Activity.Current.TraceId.ToString()
                : context.HttpContext.TraceIdentifier,
            ElapsedMilliseconds = 0,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now,
            LogLevel = Logger.LogLevel.Error,
            Route = request.Path,
            HttpMethod = request.Method,
            RawData = JsonSerializer.Serialize(new
            {
                InnerMessage = ex.InnerException?.Message,
                ex.StackTrace
            }),
            ErrorMessage = ex.Message,
            StatusCode = 500,
            UserId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserName = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value,
            UserAgent = request.Headers["User-Agent"],
            IpAddress = request.Headers["X-Real-IP"],
            AliasName = "GlobalException"
        });

        var type = ex.GetType().Name;
        const int statusCode = 500;
        context.HttpContext.Response.StatusCode = statusCode;
        const string message = "Internal Server Error";

        Activity.Current?.AddTag("errorMessage", ex.Message);
        Activity.Current?.AddTag("errorType", type);
        Activity.Current?.AddTag("errorStack", ex.StackTrace);
        Activity.Current?.SetStatus(ActivityStatusCode.Error);

        if (_environment.IsProduction())
        {
            // 生产环境，不显示异常详情
            context.Result = new ObjectResult(new
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                TraceId = traceId
            });
            await base.OnExceptionAsync(context);
            return;
        }

        // 如果是开发环境，则返回详细的错误信息
        var innerMessage = $"[{type}] {ex.Message}";
        if (ex.InnerException != null && ex.Message != ex.InnerException.Message)
        {
            innerMessage += "," + ex.InnerException.Message;
        }

        context.Result = new ObjectResult(new
        {
            InnerMessage = innerMessage,
            Message = message,
            Success = false,
            StatusCode = statusCode,
            ex.StackTrace,
            TraceId = traceId
        });
        await base.OnExceptionAsync(context);
    }
}

//
internal class ValidationResult
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; } = (int) HttpStatusCode.BadRequest;

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = "Validation Failed";

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<ValidationError> Errors { get; } = new();
}

/// <summary>
/// 
/// </summary>
internal class ValidationError
{
    public string? Field { get; }

    public string Message { get; }

    public ValidationError(string field, string message)
    {
        Field = field != string.Empty ? field : null;
        Message = message;
    }
}