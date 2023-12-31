namespace Athena.Infrastructure.SubApplication.Services.DaprImpls;

/// <summary>
/// 用户服务接口Dapr实现
/// </summary>
public class DaprUserService : DaprServiceBase, IUserService
{
    private readonly DaprClient _daprClient;
    private readonly ServiceCallConfig _config;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="daprClient"></param>
    /// <param name="accessor"></param>
    /// <param name="cacheManager"></param>
    public DaprUserService(IOptions<ServiceCallConfig> options, DaprClient daprClient,
        ISecurityContextAccessor accessor, ICacheManager cacheManager) : base(accessor)
    {
        _daprClient = daprClient;
        _cacheManager = cacheManager;
        _config = options.Value;
    }

    /// <summary>
    /// 读取外部页面列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<ExternalPageModel>> GetExternalPagesAsync(string userId)
    {
        var methodName = $"/api/SubApplication/GetExternalPages?userId={userId}";

        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<ExternalPageModel>>>(HttpMethod.Get, _config.AppId,
                GetMethodName(methodName));

        return result.Data ?? new List<ExternalPageModel>();
    }

    /// <summary>
    /// 读取用户自定表格列列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="appId">应用ID</param>
    /// <param name="moduleName">模块名称</param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<UserCustomColumnModel>> GetUserCustomColumnsAsync(
        string userId,
        string appId,
        string moduleName
    )
    {
        var methodName =
            $"/api/SubApplication/GetUserCustomColumns?userId={userId}&appId={appId}&moduleName={moduleName}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<UserCustomColumnModel>>>(HttpMethod.Get, _config.AppId,
                GetMethodName(methodName));

        if (!result.Success || result.Data == null || result.Data.Count == 0)
        {
            return new List<UserCustomColumnModel>();
        }

        return result.Data;
    }

    /// <summary>
    /// 读取用户列权限
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="moduleName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<UserColumnPermissionModel>?> GetUserColumnPermissionsAsync(string? appId, string moduleName,
        string? userId)
    {
        var methodName = $"/api/SubApplication/GetUserColumnPermissions?userId={userId}&appId={appId}&moduleName={moduleName}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<UserColumnPermissionModel>>>(HttpMethod.Get, _config.AppId,
                GetMethodName(methodName));

        if (!result.Success || result.Data == null || result.Data.Count == 0)
        {
            return new List<UserColumnPermissionModel>();
        }

        return result.Data;
    }

    /// <summary>
    /// 读取用户资源
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="appId">应用ID</param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<ResourceModel>> GetUserResourceAsync(string userId, string appId)
    {
        var methodName = $"/api/SubApplication/GetUserResource?userId={userId}&appId={appId}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<ResourceModel>>>(HttpMethod.Get, _config.AppId,
                GetMethodName(methodName));
        if (!result.Success || result.Data == null || result.Data.Count == 0)
        {
            return new List<ResourceModel>();
        }

        return result.Data;
    }

    /// <summary>
    /// 读取用户资源代码
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="appId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<string>> GetUserResourceCodesAsync(string userId, string appId)
    {
        var methodName = $"/api/SubApplication/GetUserResourceCodes?userId={userId}&appId={appId}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<List<string>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));
        if (!result.Success || result.Data == null || result.Data.Count == 0)
        {
            return new List<string>();
        }

        return result.Data;
    }

    /// <summary>
    /// 读取用户信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<GetUserInfoResponse?> GetUserInfoAsync(string userId)
    {
        var methodName = $"/api/SubApplication/GetUserInfo?userId={userId}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<GetUserInfoResponse>>(HttpMethod.Get, _config.AppId,
                GetMethodName(methodName));

        return result.Data;
    }

    /// <summary>
    /// 读取用户ID
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<string?> GetIdByUserNameAsync(string userName)
    {
        var methodName = $"/api/SubApplication/GetUserIdByUserName?userName={userName}";
        var result = await _daprClient
            .InvokeMethodAsync<ApiResult<string>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));
        return result.Data;
    }

    /// <summary>
    /// 更新表格列表信息
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<long> UpdateUserCustomColumnsAsync(UpdateUserCustomColumnsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _daprClient
            .InvokeMethodAsync<UpdateUserCustomColumnsRequest, ApiResult<long>>(
                _config.AppId,
                request.MethodName,
                request,
                cancellationToken
            );
        return result?.Data ?? 0;
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [ServiceInvokeExceptionLogging]
    public async Task<string> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _daprClient
            .InvokeMethodAsync<CreateUserRequest, ApiResult<string>>(
                _config.AppId,
                request.MethodName,
                request,
                cancellationToken
            );

        if (!result.Success || string.IsNullOrEmpty(result.Data))
        {
            throw FriendlyException.Of(result.Message);
        }

        return result.Data;
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [ServiceInvokeExceptionLogging]
    public async Task<string> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _daprClient
            .InvokeMethodAsync<UpdateUserRequest, ApiResult<string>>(
                _config.AppId,
                request.MethodName,
                request,
                cancellationToken
            );

        if (!result.Success || string.IsNullOrEmpty(result.Data))
        {
            throw FriendlyException.Of(result.Message);
        }

        return result.Data;
    }

    /// <summary>
    /// 读取用户列表
    /// </summary>
    /// <param name="readFromCache"></param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<SelectViewModel>> GetAllUserAsync(bool readFromCache = true)
    {
        if (!readFromCache)
        {
            return await Get();
        }

        const string cacheKey = "userService:GetAllUserAsync";
        return await _cacheManager.GetOrCreateAsync(cacheKey, Get, TimeSpan.FromDays(1)) ?? new List<SelectViewModel>();

        // 是否读取缓存
        async Task<List<SelectViewModel>> Get()
        {
            const string methodName = "/api/SubApplication/GetUserSelectList";
            var result = await _daprClient
                .InvokeMethodAsync<ApiResult<List<SelectViewModel>>>(HttpMethod.Get, _config.AppId, GetMethodName(methodName));
            return result.Data ?? new List<SelectViewModel>();
        }
    }
}