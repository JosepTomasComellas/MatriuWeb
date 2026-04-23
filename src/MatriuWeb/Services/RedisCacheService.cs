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
        if (!await IsAvailableAsync()) return default;
        try
        {
            var db = _redis!.GetDatabase();
            var val = await db.StringGetAsync(key);
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
        if (!await IsAvailableAsync()) return;
        try
        {
            var db = _redis!.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            if (expiry.HasValue)
                await db.StringSetAsync(key, json, expiry.Value);
            else
                await db.StringSetAsync(key, json);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Redis SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (!await IsAvailableAsync()) return;
        try
        {
            await _redis!.GetDatabase().KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Redis DEL failed for key {Key}", key);
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        if (_redis == null) return false;
        try { return _redis.IsConnected && (await _redis.GetDatabase().PingAsync()).TotalMilliseconds < 2000; }
        catch { return false; }
    }
}
