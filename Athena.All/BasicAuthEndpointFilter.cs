using System.Net.Http.Headers;
using Athena.Infrastructure.Mvc.Configs;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public class BasicAuthEndpointFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // 获取配置实例
        if (context
                .HttpContext
                .RequestServices
                .GetService(typeof(IOptions<BasicAuthConfig>)) is not IOptions<BasicAuthConfig> options)
        {
            return next(context);
        }

        // 读取配置
        var userName = options.Value.UserName;
        var password = options.Value.Password;

        // 如果配置为空，则直接跳过验证
        if (string.IsNullOrEmpty(userName) ||
            string.IsNullOrEmpty(password))
        {
            return next(context);
        }

        // 读取请求头上的验证信息
        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            context.HttpContext.Response.StatusCode = 401;
            return next(context);
        }

        // 验证请求头上的验证信息
        var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
        if (authHeaderVal.Scheme != "Basic")
        {
            context.HttpContext.Response.StatusCode = 401;
            return next(context);
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
            context.HttpContext.Response.StatusCode = 401;
            return next(context);
        }

        return next(context);
    }
}