namespace MatriuWeb.Models;

public class FrameProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public string DefaultView { get; set; } = "matrix";
    public int GlobalRefreshSeconds { get; set; } = 30;
    public List<FrameItem> Frames { get; set; } = new();

    public List<FrameItem> EnabledFrames => Frames
        .Where(f => f.Enabled)
        .OrderBy(f => f.Order)
        .ToList();

    public bool HasMinimumFrames() => Frames.Count(f => f.Enabled) >= 2;
}
