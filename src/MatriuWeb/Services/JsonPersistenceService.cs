using System.Text.Json;
using MatriuWeb.Models;
using Microsoft.Extensions.Options;

namespace MatriuWeb.Services;

public class PersistenceOptions
{
    public string ConfigPath { get; set; } = "/app/data/config/frame-config.json";
    public string BackupPath { get; set; } = "/app/data/config/backups";
    public int MaxBackups { get; set; } = 20;
}

public class JsonPersistenceService : IJsonPersistenceService
{
    private readonly PersistenceOptions _opts;
    private readonly ILogger<JsonPersistenceService> _log;
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonPersistenceService(IOptions<PersistenceOptions> opts, ILogger<JsonPersistenceService> log)
    {
        _opts = opts.Value;
        _log = log;
    }

    public async Task<FrameConfig> LoadAsync()
    {
        var path = _opts.ConfigPath;

        if (!File.Exists(path))
        {
            _log.LogWarning("Config not found at {Path}, creating default", path);
            var def = FrameConfig.CreateDefault();
            await SaveAsync(def);
            return def;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var config = JsonSerializer.Deserialize<FrameConfig>(json, _json);

            if (config == null || config.Profiles.Count == 0)
                throw new InvalidDataException("Config is null or has no profiles");

            if (config.ActiveProfile?.HasMinimumFrames() == false)
                throw new InvalidDataException("Active profile has fewer than 2 enabled frames");

            return config;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to load config from {Path}, attempting backup restore", path);
            return await TryRestoreFromBackupAsync() ?? FrameConfig.CreateDefault();
        }
    }

    public async Task SaveAsync(FrameConfig config)
    {
        await _lock.WaitAsync();
        try
        {
            var dir = Path.GetDirectoryName(_opts.ConfigPath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(config, _json);
            await File.WriteAllTextAsync(_opts.ConfigPath, json);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<FrameConfig?> TryRestoreFromBackupAsync()
    {
        if (!Directory.Exists(_opts.BackupPath)) return null;

        var backups = Directory.GetFiles(_opts.BackupPath, "*.json")
            .OrderByDescending(f => f)
            .ToList();

        foreach (var backup in backups)
        {
            try
            {
                var json = await File.ReadAllTextAsync(backup);
                var config = JsonSerializer.Deserialize<FrameConfig>(json, _json);
                if (config?.Profiles.Count > 0)
                {
                    _log.LogInformation("Restored config from backup {Backup}", backup);
                    return config;
                }
            }
            catch { /* try next */ }
        }

        return null;
    }
}
