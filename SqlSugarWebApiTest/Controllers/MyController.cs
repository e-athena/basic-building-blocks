using Athena.Infrastructure.EventStorage;
using Athena.Infrastructure.EventStorage.Messaging.Requests;
using Athena.Infrastructure.Mvc.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace SqlSugarWebApiTest.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class MyController : ControllerBase
{
    [HttpGet]
    [IgnoreApiResultFilter]
    public async Task<IActionResult> MyAction([FromServices] IEventStreamQueryService queryService, string id)
    {
        var res = await queryService.GetPagingAsync(new GetEventStreamPagingRequest
        {
            Id = id
        });
        return new JsonResult(res);
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