using Microsoft.Extensions.Options;

namespace MatriuWeb.Services;

public class BackupService : IBackupService
{
    private readonly PersistenceOptions _opts;
    private readonly ILogger<BackupService> _log;

    public BackupService(IOptions<PersistenceOptions> opts, ILogger<BackupService> log)
    {
        _opts = opts.Value;
        _log = log;
    }

    public async Task CreateBackupAsync()
    {
        if (!File.Exists(_opts.ConfigPath)) return;

        Directory.CreateDirectory(_opts.BackupPath);
        var ts = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var dest = Path.Combine(_opts.BackupPath, $"frame-config-{ts}.json");

        await Task.Run(() => File.Copy(_opts.ConfigPath, dest, overwrite: true));
        _log.LogInformation("Backup created: {Dest}", dest);

        await PruneOldBackupsAsync();
    }

    public Task<IReadOnlyList<string>> ListBackupsAsync()
    {
        if (!Directory.Exists(_opts.BackupPath))
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        var files = Directory.GetFiles(_opts.BackupPath, "*.json")
            .OrderByDescending(f => f)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(files);
    }

    public async Task RestoreBackupAsync(string backupFileName)
    {
        var src = Path.Combine(_opts.BackupPath, backupFileName);
        if (!File.Exists(src)) throw new FileNotFoundException("Backup not found", backupFileName);

        await CreateBackupAsync(); // backup of current before restoring
        await Task.Run(() => File.Copy(src, _opts.ConfigPath, overwrite: true));
        _log.LogInformation("Restored from backup: {Src}", src);
    }

    private Task PruneOldBackupsAsync()
    {
        var files = Directory.GetFiles(_opts.BackupPath, "*.json")
            .OrderByDescending(f => f)
            .Skip(_opts.MaxBackups)
            .ToList();

        return Task.Run(() => { foreach (var f in files) File.Delete(f); });
    }
}
