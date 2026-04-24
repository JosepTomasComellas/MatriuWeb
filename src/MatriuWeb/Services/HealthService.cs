using MatriuWeb.Models;
using Microsoft.Extensions.Options;

namespace MatriuWeb.Services;

public class HealthService : IHealthService
{
    private readonly IRedisCacheService _redis;
    private readonly IConfiguration _config;
    private readonly PersistenceOptions _persistence;

    public HealthService(IRedisCacheService redis, IConfiguration config, IOptions<PersistenceOptions> persistence)
    {
        _redis = redis;
        _config = config;
        _persistence = persistence.Value;
    }

    public async Task<HealthStatus> GetStatusAsync()
    {
        var redisOk = await _redis.IsAvailableAsync();
        var persistenceOk = File.Exists(_persistence.ConfigPath);
        var version = _config["App:Version"] ?? "unknown";

        var overall = redisOk && persistenceOk ? "ok" : "degraded";

        return new HealthStatus
        {
            Status = overall,
            Version = version,
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, string>
            {
                ["redis"]       = redisOk      ? "ok" : "unavailable",
                ["persistence"] = persistenceOk ? "ok" : "missing"
            }
        };
    }
}
