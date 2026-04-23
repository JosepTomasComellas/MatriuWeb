using MatriuWeb.Models;

namespace MatriuWeb.Services;

public interface IJsonPersistenceService
{
    Task<FrameConfig> LoadAsync();
    Task SaveAsync(FrameConfig config);
}
