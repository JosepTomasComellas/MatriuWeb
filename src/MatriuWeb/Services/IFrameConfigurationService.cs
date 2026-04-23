using MatriuWeb.Models;

namespace MatriuWeb.Services;

public interface IFrameConfigurationService
{
    Task<FrameConfig> GetConfigAsync();
    Task SaveConfigAsync(FrameConfig config);
    Task<FrameProfile> GetActiveProfileAsync();
    Task SetActiveProfileAsync(string profileId);
    Task AddProfileAsync(FrameProfile profile);
    Task UpdateProfileAsync(FrameProfile profile);
    Task DeleteProfileAsync(string profileId);
    Task DuplicateProfileAsync(string profileId);
    Task AddFrameAsync(string profileId, FrameItem frame);
    Task UpdateFrameAsync(string profileId, FrameItem frame);
    Task DeleteFrameAsync(string profileId, string frameId);
    Task ReorderFrameAsync(string profileId, string frameId, int direction);
    Task<string> ExportJsonAsync();
    Task ImportJsonAsync(string json);
}
