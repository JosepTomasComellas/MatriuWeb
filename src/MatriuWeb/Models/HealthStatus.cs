namespace MatriuWeb.Models;

public class HealthStatus
{
    public string Status { get; set; } = "ok";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, string> Components { get; set; } = new();
}
