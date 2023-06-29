namespace Athena.Infrastructure.ApiPermission.Attributes;

/// <summary>
/// 权限验证器
/// </summary>
public class ApiPermissionAuthorizeFilterAttribute : ActionFilterAttribute
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
                p.GetType() == typeof(AllowAnonymousAttribute) ||
                p.GetType() == typeof(SkipApiPermissionVerificationAttribute)
            );

        // 如果有跳过权限验证标签
        if (hasAllowAnonymous)
        {
            base.OnActionExecuting(context);
            return;
        }

        // 通过反射读取特性Permission，用于验证是否有访问对应Action的权限
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            base.OnActionExecuting(context);
            return;
        }

        bool IsApiPermissionAttribute(IEnumerable<Attribute> o) => o.OfType<ApiPermissionAttribute>().Any();
        var flag = IsApiPermissionAttribute(GetCustomAttributes(controllerActionDescriptor.MethodInfo, false));
        // 如果方法没有标记，直接验证通过
        if (!flag)
        {
            bool IsApiPermissionAuthorizeAttribute(IEnumerable<Attribute> o) =>
                o.OfType<ApiPermissionAuthorizeAttribute>().Any();

            // 如果类上有标记，直接验证通过
            flag = IsApiPermissionAuthorizeAttribute(
                GetCustomAttributes(controllerActionDescriptor.ControllerTypeInfo, true)
            );
            if (!flag)
            {
                base.OnActionExecuting(context);
                return;
            }
        }

        // 获取授权服务
        var contextAccessor = context
            .HttpContext
            .RequestServices
            .GetService(typeof(ISecurityContextAccessor)) as ISecurityContextAccessor;

        // 如果是开发者，则跳过权限验证
        if (contextAccessor?.UserName == "root")
        {
            base.OnActionExecuting(context);
            return;
        }

        // 读取用户ID
        var userId = contextAccessor?.UserId;
        var tenantId = contextAccessor?.TenantId;

        if (userId == null)
        {
            context.Result = new JsonResult(new
            {
                StatusCode = 401,
                Message = "用户未登录"
            });
            context.HttpContext.Response.StatusCode = 401;
            base.OnActionExecuting(context);
            return;
        }

        // 获取用户缓存服务
        var userCacheService = context
            .HttpContext
            .RequestServices
            .GetService(typeof(IApiPermissionCacheService)) as IApiPermissionCacheService;

        // 读取用户拥有的权限
        var userPermissions = userCacheService?.Get(tenantId, userId);

        // 与特性标记的对比,如果不存在,则验证失败
        if (userPermissions == null || userPermissions.Count == 0)
        {
            context.Result = new JsonResult(new
            {
                StatusCode = 403,
                Message = "操作未授权"
            });
            context.HttpContext.Response.StatusCode = 403;
            base.OnActionExecuting(context);
            return;
        }

        // 当前请求的权限信息[方法]
        var permissionInfo = controllerActionDescriptor
            .MethodInfo
            .GetCustomAttribute(typeof(ApiPermissionAttribute)) as ApiPermissionAttribute;

        // 当前操作权限列表
        var currentActionPermission = new List<string>();
        // 为空时代表是类级别的权限
        if (permissionInfo == null)
        {
            // 当前请求的控制器名称
            var controllerName = controllerActionDescriptor.ControllerTypeInfo.Name;
            // 当前请求的Action名称
            var actionName = controllerActionDescriptor.MethodInfo.Name;
            // 当前请求的权限代码
            var permissionCode = $"{controllerName}_{actionName}";
            currentActionPermission.Add(permissionCode);
        }
        else
        {
            // 如果设置了别名
            if (!string.IsNullOrEmpty(permissionInfo.Alias))
            {
                currentActionPermission.Add(permissionInfo.Alias);
            }
        }

        // 如果有额外添加规则名称
        if (permissionInfo is {AdditionalRules: { }})
        {
            currentActionPermission.AddRange(permissionInfo.AdditionalRules);
        }

        // 权限不包含当前请求的权限
        if (!userPermissions.Intersect(currentActionPermission).Any())
        {
            context.Result = new JsonResult(new
            {
                StatusCode = 403,
                Message = "操作未授权"
            });
            context.HttpContext.Response.StatusCode = 403;
        }

        base.OnActionExecuting(context);
    }
}