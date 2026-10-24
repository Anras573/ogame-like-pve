using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Game.Api.Auth;
using Game.Api.Infrastructure;
using Game.Api.Infrastructure.Repositories;
using Game.Api.Planets;
using Game.Api.Realtime;
using Game.Api.Seasons;
using Game.Api.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Game")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(Random.Shared);

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<GameDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "ogame_pve_auth";
    options.Cookie.HttpOnly = true;
    // Strict keeps the cookie off cross-site requests, which is our CSRF protection for the SPA
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

    // An API answers with status codes instead of redirecting to a login page
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var authPermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 20);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AuthEndpoints.RateLimitPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authPermitLimit,
                Window = TimeSpan.FromMinutes(1),
            }));
});

// Enums travel as names ("MetalMine") and numbers only as numbers, which keeps the API readable and the
// generated TypeScript types precise
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});
builder.Services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<ConstructionCompletionProcessor>();
if (builder.Configuration.GetValue("Worker:Enabled", true))
{
    builder.Services.AddHostedService<ConstructionCompletionWorker>();
}

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.InitializeDevelopmentDatabaseAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok()).WithTags("Health").AllowAnonymous();
app.MapAuthEndpoints();
app.MapSeasonEndpoints();
app.MapPlanetEndpoints();
app.MapHub<GameHub>(GameHub.Route);

app.Run();