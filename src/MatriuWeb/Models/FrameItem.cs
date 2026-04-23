namespace MatriuWeb.Models;

public class FrameItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int RefreshSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = true;
    public int Order { get; set; } = 0;

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Title) &&
        Uri.TryCreate(Url, UriKind.Absolute, out _);
}
