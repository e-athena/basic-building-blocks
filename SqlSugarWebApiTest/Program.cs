// // Run an Athena application.
// WebApplication
//     .CreateBuilder(args)
//     .AddAthena()
//     .Build()
//     .RunAthena<Program>();

var builder = WebApplication.CreateBuilder(args);
builder.AddAthena(services =>
{
    services.AddSubApplicationServices(builder.Configuration);
});
var app = builder.Build();
app.UseAthena<Program>(mapActions: application =>
{
    application.MapCustomSignalR();
});
app.Run();