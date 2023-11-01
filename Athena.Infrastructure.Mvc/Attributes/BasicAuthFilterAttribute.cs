using System.Net.Http.Headers;
using Athena.Infrastructure.Auth.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Athena.Infrastructure.Mvc.Attributes;

/// <summary>
/// 基础认证过滤器
/// </summary>
public class BasicAuthFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // 是否跳过权限验证
        var hasAllowAnonymous = context
            .ActionDescriptor
            .EndpointMetadata
            .Any(p =>
                p.GetType() == typeof(AllowAnonymousAttribute)
            );

        // 如果有跳过权限验证标签
        if (hasAllowAnonymous)
        {
            base.OnActionExecuting(context);
            return;
        }

        // 获取配置实例
        if (context
                .HttpContext
                .RequestServices
                .GetService(typeof(IOptions<BasicAuthConfig>)) is not IOptions<BasicAuthConfig> options)
        {
            base.OnActionExecuting(context);
            return;
        }

        // 读取配置
        var userName = options.Value.UserName;
        var password = options.Value.Password;

        // 如果配置为空，则直接跳过验证
        if (string.IsNullOrEmpty(userName) ||
            string.IsNullOrEmpty(password))
        {
            base.OnActionExecuting(context);
            return;
        }

        // 读取请求头上的验证信息
        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            context.Result = new JsonResult(new
            {
                StatusCode = 401,
                Message = "用户未授权"
            });
            context.HttpContext.Response.StatusCode = 401;
            base.OnActionExecuting(context);
            return;
        }

        // 验证请求头上的验证信息
        var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
        if (authHeaderVal.Scheme != "Basic")
        {
            context.Result = new JsonResult(new
            {
                StatusCode = 401,
                Message = "用户未授权"
            });
            context.HttpContext.Response.StatusCode = 401;
            base.OnActionExecuting(context);
            return;
        }

        // 解析请求头上的验证信息
        var credentials = Encoding.UTF8
            .GetString(Convert.FromBase64String(authHeaderVal.Parameter ?? string.Empty))
            .Split(':', 2);

        // 验证请求头上的验证信息
        if (credentials.Length != 2 ||
            credentials[0] != userName ||
            credentials[1] != password)
        {
            context.Result = new JsonResult(new
            {
                StatusCode = 401,
                Message = "用户未授权"
            });
            context.HttpContext.Response.StatusCode = 401;
            base.OnActionExecuting(context);
            return;
        }

        base.OnActionExecuting(context);
    }
}