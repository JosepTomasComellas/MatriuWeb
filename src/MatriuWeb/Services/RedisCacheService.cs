using System.Text.Json;
using StackExchange.Redis;

namespace MatriuWeb.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisCacheService> _log;

    public RedisCacheService(ILogger<RedisCacheService> log, IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _log = log;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (_redis == null || !_redis.IsConnected) return default;
        try
        {
            var val = await _redis.GetDatabase().StringGetAsync(key);
            if (!val.HasValue) return default;
            return JsonSerializer.Deserialize<T>((string)val!);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Redis GET failed for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (_redis == null || !_redis.IsConnected) return;
        try
        {
            var json = JsonSerializer.Serialize(value);
            if (expiry.HasValue)
                await _redis.GetDatabase().StringSetAsync(key, json, expiry.Value);
            else
                await _redis.GetDatabase().StringSetAsync(key, json);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (_redis == null || !_redis.IsConnected) return;
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Redis DEL failed for key {Key}", key);
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (_redis == null || !_redis.IsConnected) return false;
        try { return (await _redis.GetDatabase().PingAsync()).TotalMilliseconds < 2000; }
        catch { return false; }
    }
}
