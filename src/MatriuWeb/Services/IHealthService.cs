using MatriuWeb.Models;

namespace MatriuWeb.Services;

public interface IHealthService
{
    Task<HealthStatus> GetStatusAsync();
}
