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
IConnectionMultiplexer? redisMux = null;
try { redisMux = ConnectionMultiplexer.Connect(redisConn); } catch { /* degradació graceful */ }
if (redisMux != null)
    builder.Services.AddSingleton<IConnectionMultiplexer>(redisMux);

// HttpClient per a les proves d'iframe (ignora errors de certificat en entorns interns)
builder.Services.AddHttpClient("iframe-probe")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 3
    });

// Services
builder.Services.AddSingleton<IJsonPersistenceService, JsonPersistenceService>();
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddSingleton<IFrameConfigurationService, FrameConfigurationService>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddSingleton<IIframeStatusService, IframeStatusService>();
builder.Services.AddScoped<IDashboardStateService, DashboardStateService>();
builder.Services.AddSingleton<IHealthService, HealthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MatriuWeb.Components.App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/health", async (IHealthService health) =>
{
    var status = await health.GetStatusAsync();
    return Results.Ok(status);
});

app.Run();
