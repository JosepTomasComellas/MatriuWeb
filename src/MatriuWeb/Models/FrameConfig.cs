namespace MatriuWeb.Models;

public class FrameConfig
{
    public string ActiveProfileId { get; set; } = "default";
    public List<FrameProfile> Profiles { get; set; } = new();

    public FrameProfile? ActiveProfile =>
        Profiles.FirstOrDefault(p => p.Id == ActiveProfileId)
        ?? Profiles.FirstOrDefault();

    public static FrameConfig CreateDefault() => new()
    {
        ActiveProfileId = "default",
        Profiles = new List<FrameProfile>
        {
            new()
            {
                Id = "default",
                Name = "Perfil principal",
                DefaultView = "matrix",
                GlobalRefreshSeconds = 30,
                Frames = new List<FrameItem>
                {
                    new() { Id = "frame-1", Title = "Dashboard 1", Url = "https://example.com", Order = 1 },
                    new() { Id = "frame-2", Title = "Dashboard 2", Url = "https://example.com", Order = 2 }
                }
            }
        }
    };
}
