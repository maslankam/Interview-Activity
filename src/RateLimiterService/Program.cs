using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using RateLimiterService.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddDbContext<RateLimitDbContext>(options =>
{
    options.UseInMemoryDatabase("RateLimiterDb");
});
builder.Services.AddSingleton<RateLimiter>();
builder.Services.AddScoped<RateLimiterService.Services.RateLimiterServiceImpl>();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();
app.MapGrpcService<RateLimiterService.Services.RateLimiterServiceImpl>();
app.MapGet("/", () => "RateLimiter gRPC service running.");

app.Run();
