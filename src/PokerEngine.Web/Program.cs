using PokerEngine.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPokerEndpoints();
app.MapFallbackToFile("index.html");

app.Run();
