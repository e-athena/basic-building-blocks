using Athena.Infrastructure.Domain;
using Athena.Infrastructure.FreeSql.Bases;
using Athena.Infrastructure.FreeSql.Tenants;
using Athena.Infrastructure.FreeSql.Test.Domain;
using FreeSql;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Infrastructure.FreeSql.Test;

public class UnitTest1 : TestBase
{
    [Fact]
    public void Test1()
    {
        var freeSql = GetService<IFreeSql>();
        freeSql?.SyncStructure(typeof(User));
        Xunit.Assert.True(true);
    }

    // 

    protected override void RegistryServices(IServiceCollection services)
    {
        services.AddScoped<IUserQueryService, UserQueryService>();
    }

    [Fact]
    public async Task InsertWithTenantSucceed_Test1()
    {
        var mediator = GetService<IMediator>();
        var user = await mediator!.Send(new CreateUserRequest
        {
            // 随机生成一个name
            Name = Guid.NewGuid().ToString("N"),
            Age = 18
        });

        Xunit.Assert.NotNull(user);
        Xunit.Assert.Equal("test_tenant", user.TenantId);


        var service = GetService<IUserQueryService>();
        var list = await service?.GetListAsync()!;
        Xunit.Assert.True(list.Count > 0);
    }

    [Fact]
    public async Task InsertWithTenantSucceed_Test2()
    {
        var mediator = GetService<IMediator>();
        var user = await mediator!.Send(new CreateUserRequest
        {
            // 随机生成一个name
            Name = Guid.NewGuid().ToString("N"),
            Age = 18
        });

        Xunit.Assert.NotNull(user);
        Xunit.Assert.Null(user.TenantId);


        var service = GetService<IUserQueryService>();
        var list = await service?.GetListAsync()!;
        Xunit.Assert.True(list.Count > 0);
    }
}

public interface IUserQueryService
{
    // 读取列表
    Task<List<User>> GetListAsync();
}

public class UserQueryService : QueryServiceBase<User>, IUserQueryService
{
    private const string? tenantId = "test_tenant";
    // private const string? tenantId = null;

    public UserQueryService(FreeSqlMultiTenancy freeSql) : base(freeSql, new DefaultSecurityContextAccessor(tenantId))
    {
    }

    public Task<List<User>> GetListAsync()
    {
        var sql = MainQueryableNoTracking.ToSql();
        var sql1 = Queryable.ToSql();
        return Query().ToListAsync();
    }
}

public class UserRequestHandler : ServiceBase<User>, IRequestHandler<CreateUserRequest, User>
{
    private const string? tenantId = "test_tenant";
    // private const string? tenantId = null;

    public UserRequestHandler(UnitOfWorkManager unitOfWorkManager) : base(unitOfWorkManager,
        new DefaultSecurityContextAccessor(tenantId))
    {
    }

    public async Task<User> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var entity = new User
        {
            Name = request.Name,
            Age = request.Age
        };
        await RegisterNewAsync(entity, cancellationToken);
        return entity;
    }
}

public class CreateUserRequest : ITxRequest<User>
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}