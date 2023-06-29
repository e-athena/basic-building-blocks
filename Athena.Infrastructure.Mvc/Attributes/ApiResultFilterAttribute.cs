using Athena.Infrastructure.Messaging.Responses;

namespace Athena.Infrastructure.Mvc.Attributes;

/// <summary>
/// API结果统一处理过滤器
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ApiResultFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // 如果数据抛异常了，则直接返回
        if (context.Exception != null)
        {
            base.OnActionExecuted(context);
            return;
        }

        // 打了Ignore标记则跳过
        var ignore = context
            .ActionDescriptor
            .EndpointMetadata.Any(p => p.GetType().FullName == typeof(IgnoreApiResultFilterAttribute).FullName);

        // 跳过统一包装
        if (ignore)
        {
            base.OnActionExecuted(context);
            return;
        }

        var traceId = Activity.Current != null
            ? Activity.Current.TraceId.ToString()
            : context.HttpContext.TraceIdentifier;
        var objectResult = context.Result as ObjectResult;

        // 如果状态码不是200，则代表异常
        if (objectResult is {StatusCode: not null} && objectResult.StatusCode != 200)
        {
            var message = HttpStatusCodeHelper.GetDescription((int) objectResult.StatusCode);
            context.Result = new ObjectResult(new
            {
                objectResult.StatusCode,
                Message = message,
                Success = false,
                TraceId = traceId
            });
            // 设置响应状态码
            context.HttpContext.Response.StatusCode = (int) objectResult.StatusCode;

            base.OnActionExecuted(context);
            return;
        }

        // 获取结果集
        var result = objectResult?.Value;

        // 如果做了统一返回，则不需要再进行统一返回了
        var isApiResult = result is ApiResultBase;

        context.Result = new ObjectResult(isApiResult
            ? result
            : new
            {
                context.HttpContext.Response.StatusCode,
                Data = result,
                Message = "Ok",
                Success = true,
                TraceId = traceId
            });

        base.OnActionExecuted(context);
    }
}