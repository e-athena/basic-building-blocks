using Athena.Infrastructure.Mvc.Middlewares.MiddlewareInjectors;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

namespace FreeSqlWebApiTest.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    /// <summary>
    /// 注入一个测试的中间件
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("test")]
    public Task<string> Test([FromServices] MiddlewareInjectorOptions options)
    {
        options.InjectMiddleware(builder =>
        {
            // aa
            builder.UseMiddleware<TestMiddleware>("test");
            builder.Map("/testpath", subBuilder =>
            {
                subBuilder.Run(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("Injected content<br>");
                    await context.Response.WriteAsync("<a href=\"/\">Home</a><br>");
                    await context.Response.WriteAsync("</body></html>");
                });
            });
        });
        return Task.FromResult("succeed");
    }

    /// <summary>
    /// 注入一个测试的中间件
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("test1")]
    public async Task<string> Test1()
    {
        const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYxZjdkYmY2ZDc0YTQ2NDU4ZmNhMDkwMyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IiIsIlJvbGVOYW1lIjoiIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6InJvb3QiLCJSZWFsTmFtZSI6IuW8gOWPkeiAhSIsIklzVGVuYW50QWRtaW4iOiJmYWxzZSIsIlRlbmFudElkIjoiIiwibmJmIjoiMTY5Nzg4OTEwOCIsImlhdCI6IjE2OTc4ODkxMDgiLCJqdGkiOiIyOGJkZjMwYmY4MGE0MTViOGEyNGEwNDJlOWFkZWZlNSIsIkFwcElkIjoiIiwiZXhwIjoxNjk3OTc1NTA4LCJpc3MiOiJiYXNpYy1wbGF0Zm9ybS1zc28tY2VudGVyIiwiYXVkIjoiQmFzaWNQbGF0Zm9ybS5XZWJBUEkifQ.uzuzrqh5aFk0wQ3Eh8jPK-LHuU5g9bdEQVcIDAmsKoY";
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5132/hubs/event", opts =>
            {
                opts.Transports = HttpTransportType.WebSockets;
                opts.SkipNegotiation = true;
                opts.Headers = new Dictionary<string, string>
                {
                    {
                        "Authorization", token
                    }
                };
                // opts.AccessTokenProvider = async () => await Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();
        await connection.StartAsync();

        return await Task.FromResult("succeed");
    }
}

// 编写一个测试的中间件
public class TestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _str;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="str"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TestMiddleware(RequestDelegate next, string str)
    {
        _str = str ?? throw new ArgumentNullException(nameof(str));
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public Task Invoke(HttpContext httpContext)
    {
        Console.WriteLine($"TestMiddleware {_str}");
        return _next(httpContext);
    }
}