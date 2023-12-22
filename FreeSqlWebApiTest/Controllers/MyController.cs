using Athena.Infrastructure.Auths;
using Athena.Infrastructure.EventStorage;
using Athena.Infrastructure.EventStorage.Messaging.Requests;
using Athena.Infrastructure.Mvc.Attributes;
using Athena.Infrastructure.SubApplication.Aop;
using Athena.Infrastructure.SubApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FreeSqlWebApiTest.Controllers;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

[ApiController]
[Route("api/[controller]/[action]")]
public class MyController : ControllerBase
{
    private readonly IFreeSql _freeSql;

    public MyController(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    [HttpGet]
    [IgnoreApiResultFilter]
    public IActionResult MyAction([FromServices] IEventStreamQueryService queryService, string id)
    {
        return new JsonResult(queryService.GetPagingAsync(new GetEventStreamPagingRequest
        {
            Id = id
        }));
    }

    [ServiceInvokeExceptionLogging]
    [HttpGet]
    [IgnoreApiResultFilter]
    public List<string> MyAction1()
    {
        throw new Exception("sss");
    }

    [ServiceInvokeExceptionLogging]
    [HttpGet]
    [IgnoreApiResultFilter]
    public Task<List<string>> MyAction2()
    {
        throw new Exception("sss");
    }

    [ServiceInvokeExceptionLogging]
    [HttpGet]
    [IgnoreApiResultFilter]
    public List<string>? MyAction3()
    {
        throw new Exception("sss");
    }

    [ServiceInvokeExceptionLogging]
    [HttpGet]
    [IgnoreApiResultFilter]
    public Task<List<string>?> MyAction4()
    {
        throw new Exception("sss");
    }

    [ServiceInvokeExceptionLogging]
    [HttpGet]
    [IgnoreApiResultFilter]
    public dynamic? MyAction5()
    {
        throw new Exception("sss");
    }

    [ServiceInvokeExceptionLogging]
    [HttpGet]
    [IgnoreApiResultFilter]
    public Task<dynamic?> MyAction6()
    {
        throw new Exception("sss");
    }

    // [HttpGet]
    // public IActionResult MyAction(
    //     [FromServices] IUserService service,
    //     [FromServices] ISecurityContextAccessor accessor
    // )
    // {
    //     var a = service.GetUserInfoAsync(accessor.UserId!).Result;
    //     return Ok("on");
    // }
}