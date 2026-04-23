using MatriuWeb.Models;
using Microsoft.Extensions.Options;

namespace MatriuWeb.Services;

public class HealthService : IHealthService
{
    private readonly IRedisCacheService _redis;
    private readonly IConfiguration _config;

    public HealthService(IRedisCacheService redis, IConfiguration config)
    {
        _redis = redis;
        _config = config;
    }

    public async Task<HealthStatus> GetStatusAsync()
    {
        var redisOk = await _redis.IsAvailableAsync();
        var version = _config["App:Version"] ?? "unknown";

        return new HealthStatus
        {
            Status = redisOk ? "ok" : "degraded",
            Version = version,
            Timestamp = DateTime.UtcNow,
            Components = new Dictionary<string, string>
            {
                ["redis"] = redisOk ? "ok" : "unavailable",
                ["persistence"] = "ok"
            }
        };
    }
}
