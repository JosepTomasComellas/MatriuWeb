namespace MatriuWeb.Services;

public interface IBackupService
{
    Task CreateBackupAsync();
    Task<IReadOnlyList<string>> ListBackupsAsync();
    Task RestoreBackupAsync(string backupFileName);
}
