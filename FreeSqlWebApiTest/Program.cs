// // Run an Athena application.
// WebApplication
//     .CreateBuilder(args)
//     .AddAthena()
//     .Build()
//     .RunAthena<Program>();

// Run an Athena application.

using FreeSqlWebApiTest.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.AddAthena(services =>
{
    // 
    // services.AddCustomMiddlewareInjector();
    services.AddSubApplicationServices(builder.Configuration);
    services.AddCustomHangfireWithRedis(builder.Configuration);
});
var app = builder.Build();
app.UseAthena<Program>(mapActions: application =>
{
    // 
    // application.UseMiddleware<TestMiddleware>("test");
    // application.UseMiddleware<TestMiddleware1>("test1");
    // application.UseCustomMiddlewareInjector();
    application.UseCustomHangfireDashboard(builder.Configuration);
    application.MapCustomSignalR();
});
app.Run();