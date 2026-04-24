using System.Text.Json;
using MatriuWeb.Models;

namespace MatriuWeb.Services;

public class FrameConfigurationService : IFrameConfigurationService
{
    private const string CacheKey = "matriuweb:config";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IJsonPersistenceService _persistence;
    private readonly IBackupService _backup;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<FrameConfigurationService> _log;
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public FrameConfigurationService(
        IJsonPersistenceService persistence,
        IBackupService backup,
        IRedisCacheService cache,
        ILogger<FrameConfigurationService> log)
    {
        _persistence = persistence;
        _backup = backup;
        _cache = cache;
        _log = log;
    }

    public async Task<FrameConfig> GetConfigAsync()
    {
        var cached = await _cache.GetAsync<FrameConfig>(CacheKey);
        if (cached != null) return cached;

        var config = await _persistence.LoadAsync();
        await _cache.SetAsync(CacheKey, config, CacheTtl);
        return config;
    }

    public async Task SaveConfigAsync(FrameConfig config)
    {
        await _backup.CreateBackupAsync();
        await _persistence.SaveAsync(config);
        await _cache.RemoveAsync(CacheKey);
    }

    public async Task<FrameProfile> GetActiveProfileAsync()
    {
        var config = await GetConfigAsync();
        return config.ActiveProfile ?? config.Profiles.First();
    }

    public async Task SetActiveProfileAsync(string profileId)
    {
        var config = await _persistence.LoadAsync();
        if (!config.Profiles.Any(p => p.Id == profileId))
            throw new InvalidOperationException($"Profile {profileId} not found");
        config.ActiveProfileId = profileId;
        await SaveConfigAsync(config);
    }

    public async Task AddProfileAsync(FrameProfile profile)
    {
        var config = await _persistence.LoadAsync();
        config.Profiles.Add(profile);
        await SaveConfigAsync(config);
    }

    public async Task UpdateProfileAsync(FrameProfile profile)
    {
        var config = await _persistence.LoadAsync();
        var idx = config.Profiles.FindIndex(p => p.Id == profile.Id);
        if (idx < 0) throw new InvalidOperationException($"Profile {profile.Id} not found");
        config.Profiles[idx] = profile;
        await SaveConfigAsync(config);
    }

    public async Task DeleteProfileAsync(string profileId)
    {
        var config = await _persistence.LoadAsync();
        if (config.Profiles.Count <= 1) throw new InvalidOperationException("Cannot delete the last profile");
        config.Profiles.RemoveAll(p => p.Id == profileId);
        if (config.ActiveProfileId == profileId)
            config.ActiveProfileId = config.Profiles.First().Id;
        await SaveConfigAsync(config);
    }

    public async Task DuplicateProfileAsync(string profileId)
    {
        var config = await _persistence.LoadAsync();
        var src = config.Profiles.FirstOrDefault(p => p.Id == profileId)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        var copy = new FrameProfile
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Name = $"{src.Name} (còpia)",
            DefaultView = src.DefaultView,
            GlobalRefreshSeconds = src.GlobalRefreshSeconds,
            Frames = src.Frames.Select(f => new FrameItem
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Title = f.Title,
                Url = f.Url,
                RefreshSeconds = f.RefreshSeconds,
                Enabled = f.Enabled,
                Order = f.Order
            }).ToList()
        };

        config.Profiles.Add(copy);
        await SaveConfigAsync(config);
    }

    public async Task AddFrameAsync(string profileId, FrameItem frame)
    {
        var config = await _persistence.LoadAsync();
        var profile = config.Profiles.FirstOrDefault(p => p.Id == profileId)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        frame.Order = profile.Frames.Count > 0 ? profile.Frames.Max(f => f.Order) + 1 : 1;
        profile.Frames.Add(frame);
        await SaveConfigAsync(config);
    }

    public async Task UpdateFrameAsync(string profileId, FrameItem frame)
    {
        var config = await _persistence.LoadAsync();
        var profile = config.Profiles.FirstOrDefault(p => p.Id == profileId)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        var idx = profile.Frames.FindIndex(f => f.Id == frame.Id);
        if (idx < 0) throw new InvalidOperationException($"Frame {frame.Id} not found");
        profile.Frames[idx] = frame;
        await SaveConfigAsync(config);
    }

    public async Task DeleteFrameAsync(string profileId, string frameId)
    {
        var config = await _persistence.LoadAsync();
        var profile = config.Profiles.FirstOrDefault(p => p.Id == profileId)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        if (profile.Frames.Count(f => f.Enabled) <= 2 &&
            profile.Frames.FirstOrDefault(f => f.Id == frameId)?.Enabled == true)
            throw new InvalidOperationException("Mínim de 2 frames actius requerits");

        profile.Frames.RemoveAll(f => f.Id == frameId);
        RenormalizeOrder(profile);
        await SaveConfigAsync(config);
    }

    public async Task ReorderFrameAsync(string profileId, string frameId, int direction)
    {
        var config = await _persistence.LoadAsync();
        var profile = config.Profiles.FirstOrDefault(p => p.Id == profileId)
            ?? throw new InvalidOperationException($"Profile {profileId} not found");

        var frames = profile.Frames.OrderBy(f => f.Order).ToList();
        var idx = frames.FindIndex(f => f.Id == frameId);
        var newIdx = idx + direction;

        if (newIdx < 0 || newIdx >= frames.Count) return;

        (frames[idx].Order, frames[newIdx].Order) = (frames[newIdx].Order, frames[idx].Order);
        await SaveConfigAsync(config);
    }

    public async Task<string> ExportJsonAsync()
    {
        var config = await GetConfigAsync();
        return JsonSerializer.Serialize(config, _json);
    }

    public async Task ImportJsonAsync(string json)
    {
        var config = JsonSerializer.Deserialize<FrameConfig>(json, _json)
            ?? throw new InvalidDataException("JSON invàlid");

        if (config.Profiles.Count == 0)
            throw new InvalidDataException("Mínim d'un perfil requerit");

        foreach (var profile in config.Profiles)
            if (!profile.HasMinimumFrames())
                throw new InvalidDataException($"El perfil '{profile.Name}' necessita mínim 2 frames actius");

        await SaveConfigAsync(config);
    }

    private static void RenormalizeOrder(FrameProfile profile)
    {
        int order = 1;
        foreach (var f in profile.Frames.OrderBy(f => f.Order))
            f.Order = order++;
    }
}
