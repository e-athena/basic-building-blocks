namespace Athena.Infrastructure.SubApplication.Services.Impls;

/// <summary>
/// 用户服务默认实现
/// </summary>
public class DefaultUserService : DefaultServiceBase, IUserService
{
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    /// <param name="cacheManager"></param>
    public DefaultUserService(ISecurityContextAccessor accessor, ICacheManager cacheManager) : base(accessor)
    {
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 读取外部页面列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    [ServiceInvokeExceptionLogging]
    public async Task<List<ExternalPageModel>> GetExternalPagesAsync(string userId)
    {
        const string url = "/api/SubApplication/GetExternalPages";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userId
            })
            .GetJsonAsync<ApiResult<List<ExternalPageModel>>>();
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
        const string url = "/api/SubApplication/GetUserCustomColumns";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userId,
                appId,
                moduleName
            })
            .GetJsonAsync<ApiResult<List<UserCustomColumnModel>>>();
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
        const string url = "/api/SubApplication/GetUserColumnPermissions";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userId,
                appId,
                moduleName
            })
            .GetJsonAsync<ApiResult<List<UserColumnPermissionModel>>>();
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
        const string url = "/api/SubApplication/GetUserResource";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userId,
                appId
            })
            .GetJsonAsync<ApiResult<List<ResourceModel>>>();
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
        const string url = "/api/SubApplication/GetUserResourceCodes";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userId,
                appId
            })
            .GetJsonAsync<ApiResult<List<string>>>();
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
        const string url = "/api/SubApplication/GetUserInfo";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userId,
            })
            .GetJsonAsync<ApiResult<GetUserInfoResponse>>();

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
        const string url = "/api/SubApplication/GetUserIdByUserName";
        var result = await GetRequest(url)
            .SetQueryParams(new
            {
                userName
            })
            .GetJsonAsync<ApiResult<string>>();
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
        const string url = "/api/SubApplication/UpdateUserCustomColumns";
        var result = await GetRequest(url)
            .PostJsonAsync(request, cancellationToken: cancellationToken)
            .ReceiveJson<ApiResult<long>>();
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
        var result = await GetRequest(request.MethodName)
            .PostJsonAsync(request, cancellationToken: cancellationToken)
            .ReceiveJson<ApiResult<string>>();

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
        const string url = "/api/SubApplication/UpdateUser";
        var result = await GetRequest(url)
            .PostJsonAsync(request, cancellationToken: cancellationToken)
            .ReceiveJson<ApiResult<string>>();

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
    /// <exception cref="NotImplementedException"></exception>
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
            const string url = "/api/SubApplication/GetUserSelectList";
            var result = await GetRequest(url)
                .GetJsonAsync<ApiResult<List<SelectViewModel>>>();
            return result.Data ?? new List<SelectViewModel>();
        }
    }
}