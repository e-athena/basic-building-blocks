using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Athena.Infrastructure.ApiPermission.Attributes;
using Athena.Infrastructure.Attributes;
using Athena.Infrastructure.Auths;
using Athena.Infrastructure.Domain;
using Athena.Infrastructure.Event;
using Athena.Infrastructure.Event.Attributes;
using Athena.Infrastructure.Event.Interfaces;
using Athena.Infrastructure.EventTracking.Aop;
using Athena.Infrastructure.Messaging.Requests;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.SqlSugar;
using Athena.Infrastructure.SqlSugar.Bases;
using Athena.Infrastructure.SqlSugar.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace SqlSugarWebApiTest.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[FrontEndRouting("用户管理",
    ModuleCode = "system",
    ModuleName = "系统管理",
    ModuleRoutePath = "/system",
    ModuleSort = 1,
    RoutePath = "/system/user",
    Sort = 0,
    Description = "系统基于角色授权，每个角色对不同的功能模块具备添删改查以及自定义权限等多种权限设定"
)]
[ApiPermissionAuthorize]
public class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult Sync([FromServices] ISqlSugarClient client)
    {
        client.SyncStructure<Program>();
        return Ok();
    }

    [HttpPost]
    public async Task<string> CreateAsync([FromServices] ISender sender, CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return await sender.Send(request, cancellationToken);
    }

    [HttpPost]
    public async Task<Paging<UserInfo>> GetPagingAsync([FromServices] IUserQueryService service,
        GetUserPagingRequest request, CancellationToken cancellationToken)
    {
        return await service.GetPagingAsync(request, cancellationToken);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(ApiResult<int>), StatusCodes.Status200OK)]
    public Task<bool> DeleteAsync([FromServices] ISender sender, DeleteUserRequest request,
        CancellationToken cancellationToken)
    {
        return sender.Send(request, cancellationToken);
    }
}

[Table("users")]
public class User : FullEntityCore
{
    /// <summary>
    /// 
    /// </summary>
    [MaxLength(32)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    public int Age { get; set; }

    public string? Email { get; set; }

    public User()
    {
    }

    public User(string name, int age)
    {
        Name = name;
        Age = age;

        ApplyEvent(new UserCreatedEvent(name, age));
    }

    public void SoftDelete()
    {
        ApplyEvent(new UserDeletedEvent());
    }
}

public class UserDeletedEvent : EventBase
{
}

public class UserCreatedEvent : EventBase
{
    public string Name { get; set; }
    public int Age { get; set; }

    public UserCreatedEvent(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

// 创建用户请求类
public class CreateUserRequest : ITxRequest<string>
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}

/// <summary>
///
/// </summary>
public class DeleteUserRequest : IdRequest, ITxRequest<bool>
{
}

public class UserRequestHandler : DataPermissionServiceBase<User>,
    IRequestHandler<CreateUserRequest, string>,
    IMessageHandler<UserCreatedEvent>,
    IRequestHandler<DeleteUserRequest, bool>
{
    public UserRequestHandler(ISqlSugarClient sqlSugarClient, ISecurityContextAccessor accessor) : base(sqlSugarClient,
        accessor)
    {
    }

    public async Task<string> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var entity = new User(request.Name, request.Age);
        await RegisterNewAsync(entity, cancellationToken);
        return entity.Id;
    }

    [EventTracking]
    [IntegratedEventSubscribe(nameof(UserCreatedEvent), nameof(UserRequestHandler))]
    public Task HandleAsync(UserCreatedEvent payload, CancellationToken cancellationToken)
    {
        Console.WriteLine($"UserCreatedEvent: {payload.Name}, {payload.Age}");
        return Task.CompletedTask;
    }

    public async Task<bool> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        var entity = await GetAsync(request.Id, cancellationToken);
        entity.SoftDelete();
        return await RegisterSoftDeleteAsync(entity, cancellationToken);
    }
}

public class GetUserPagingRequest : GetPagingRequestBase
{
}

public interface IUserQueryService
{
    Task<Paging<UserInfo>> GetPagingAsync(GetUserPagingRequest request, CancellationToken cancellationToken = default);
}

[Component]
public class UserQueryService : QueryServiceBase<User>, IUserQueryService
{
    public UserQueryService(ISqlSugarClient sqlSugarClient, ISecurityContextAccessor accessor) : base(sqlSugarClient,
        accessor)
    {
    }

    public Task<Paging<UserInfo>> GetPagingAsync(GetUserPagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameter = Expression.Parameter(typeof(User), "p");
        // var property1 = Expression.Constant("select Id from users");
        var property1 = Expression.Constant("6408512d46a0d96796cc730c,6409e071bd3497127f4eb984");
        var left1 = Expression.Property(parameter, "Id");
        // var method1 = typeof(DbFunc).GetMethod("FormatSubQuery", new[] {typeof(User)});
        // var method1 = typeof(DbFunc).GetMethods()
        //     .Single(m => m is {Name: "FormatSubQuery", IsGenericMethodDefinition: true})
        //     .MakeGenericMethod(typeof(string),typeof(string));
        var method1 = typeof(DbFunc).GetMethod("FormatLeftJoin", new[] {typeof(string), typeof(string)});
        var expression1 = Expression.Call(null, method1!, property1, left1);
        var exp1 = Expression.Lambda<Func<User, bool>>(expression1, parameter);

        var sql1 = DbContext.Queryable<User>()
            .Where(exp1)
            .ToSqlString();
        // var a = new List<string> {"1", "2"};
        // var sql = DbContext.Queryable<OrganizationalUnitAuth>()
        //     .AS("business_org_auths")
        //     .Where(p => a.Contains(p.OrganizationalUnitId))
        //     .Where(p => p.BusinessTable == "User")
        //     .Select(p => p.BusinessId)
        //     .ToSqlString();

        return QueryableNoTracking
            .HasWhere(request.Keyword, p => p.Name.Contains(request.Keyword!))
            .Where(exp1)
            .ToPagingAsync(request, p => new UserInfo
            {
                Name = p.Name
            }, cancellationToken: cancellationToken);
    }
}

public class UserInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}