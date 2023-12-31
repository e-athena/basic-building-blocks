using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Athena.Infrastructure.ApiPermission.Attributes;
using Athena.Infrastructure.Attributes;
using Athena.Infrastructure.Auths;
using Athena.Infrastructure.DataAnnotations;
using Athena.Infrastructure.Domain;
using Athena.Infrastructure.Event;
using Athena.Infrastructure.Event.Attributes;
using Athena.Infrastructure.Event.Interfaces;
using Athena.Infrastructure.EventTracking.Aop;
using Athena.Infrastructure.FreeSql;
using Athena.Infrastructure.FreeSql.Bases;
using Athena.Infrastructure.FreeSql.Tenants;
using Athena.Infrastructure.Messaging.Requests;
using Athena.Infrastructure.Messaging.Responses;
using Athena.Infrastructure.Mvc;
using Athena.Infrastructure.QueryFilters;
using FluentValidation;
using FreeSql;
using FreeSql.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreeSqlWebApiTest.Controllers;

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
// [ApiPermissionAuthorizeFilter]
public class UserController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CustomBadRequestResult), StatusCodes.Status400BadRequest)]
    public async Task<string> CreateAsync([FromServices] ISender sender, CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return await sender.Send(request, cancellationToken);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(ApiResult<int>), StatusCodes.Status200OK)]
    public async Task<int> DeleteAsync([FromServices] ISender sender, DeleteUserRequest request,
        CancellationToken cancellationToken)
    {
        return await sender.Send(request, cancellationToken);
    }

    [HttpPost]
    public async Task<Paging<User>> GetPagingAsync([FromServices] IUserQueryService service,
        GetUserPagingRequest request, CancellationToken cancellationToken)
    {
        return await service.GetPagingAsync(request, cancellationToken);
    }
}

[System.ComponentModel.DataAnnotations.Schema.Table("users")]
[Athena.Infrastructure.DataAnnotations.Schema.Index(nameof(Name), IsUnique = false)]
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
    /// <summary>
    /// 
    /// </summary>
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
    /// <summary>
    /// 姓名
    /// </summary>
    [MinLength(2, ErrorMessage = "姓名不能小于2个字符")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 年龄
    /// </summary>
    [Range(16, 100, ErrorMessage = "年龄必须大于16岁~")]
    public int Age { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱错啦")]
    public string? Email { get; set; }

    //手机号
    [PhoneNumber] public string? PhoneNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [IdCard]
    public string? Test1 { get; set; }
}

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    /// <summary>
    /// 
    /// </summary>
    public CreateUserValidator()
    {
        // RuleFor(p => p.Age)
        //     .Must(age => age > 16)
        //     .WithMessage("年龄必须大于16岁");
        // 验证邮箱格式
        RuleFor(p => p.Email)
            .EmailAddress()
            .WithMessage("邮箱格式不正确");
    }
}

public class DeleteUserRequest : IdRequest, ITxRequest<int>
{
}

public class UserRequestHandler : DataPermissionServiceBase<User>,
    IRequestHandler<CreateUserRequest, string>,
    IMessageHandler<UserCreatedEvent>,
    IRequestHandler<DeleteUserRequest, int>
{
    public UserRequestHandler(UnitOfWorkManager unitOfWorkManager, ISecurityContextAccessor accessor) : base(
        unitOfWorkManager, accessor)
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

    public async Task<int> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
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
    Task<Paging<User>> GetPagingAsync(GetUserPagingRequest request, CancellationToken cancellationToken = default);
}

[Component]
public class UserQueryService : DataPermissionQueryServiceBase<User>, IUserQueryService
{
    public UserQueryService(IFreeSql freeSql, ISecurityContextAccessor accessor) : base(freeSql,
        accessor)
    {
    }

    public Task<Paging<User>> GetPagingAsync(GetUserPagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameter = Expression.Parameter(typeof(User), "p");

        var a = new List<string> {"1", "2"};
        var sql = FreeSqlDbContext.Select<OrganizationalUnitAuth>()
            .AsTable((_, _) => "business_org_auths")
            .Where(p => a.Contains(p.OrganizationalUnitId))
            .Where(p => p.BusinessTable == "User")
            .ToSql(p => p.BusinessId, FieldAliasOptions.AsProperty);

        var property1 =
            Expression.Constant("");
        var left1 = Expression.Property(parameter, "Id");
        var method1 = typeof(DbFunc).GetMethod("FormatLeftJoin", new[] {typeof(string), typeof(string)});
        var expression1 = Expression.Call(null, method1!, property1, left1);
        var exp1 = Expression.Lambda<Func<User, bool>>(expression1, parameter);

        // var sql1 = FreeSqlDbContext.Queryable<User>()
        //     .Where(exp1)
        //     .ToSql();

        return QueryableNoTracking
            .HasWhere(request.Keyword, p => p.Name.Contains(request.Keyword!))
            // .Where(exp1)
            .ToPagingAsync(UserId, request, cancellationToken: cancellationToken);
    }
}