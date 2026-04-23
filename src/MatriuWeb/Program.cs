using MatriuWeb.Services;
using MudBlazor.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Options
builder.Services.Configure<PersistenceOptions>(
    builder.Configuration.GetSection("Persistence"));

// Redis (opcional, degradació graceful si no disponible)
var redisConn = builder.Configuration["Redis:ConnectionString"] ?? "redis:6379";
try
{
    var mux = ConnectionMultiplexer.Connect(redisConn);
    builder.Services.AddSingleton<IConnectionMultiplexer>(mux);
}
catch
{
    builder.Services.AddSingleton<IConnectionMultiplexer?>(_ => null);
}

// Services
builder.Services.AddSingleton<IJsonPersistenceService, JsonPersistenceService>();
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddSingleton<IFrameConfigurationService, FrameConfigurationService>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IDashboardStateService, DashboardStateService>();
builder.Services.AddSingleton<IHealthService, HealthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MatriuWeb.Components.App>()
    .AddInteractiveServerRenderMode();

// Health endpoint (simple JSON, no dep externa)
app.MapGet("/health", async (IHealthService health) =>
{
    var status = await health.GetStatusAsync();
    return Results.Ok(status);
});

app.Run();
