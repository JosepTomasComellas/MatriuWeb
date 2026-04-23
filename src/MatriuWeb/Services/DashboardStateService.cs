namespace MatriuWeb.Services;

public class DashboardStateService : IDashboardStateService
{
    public string? FocusedFrameId { get; private set; }
    public bool IsRefreshPaused { get; private set; }
    public event Action? StateChanged;

    public void FocusFrame(string? frameId)
    {
        FocusedFrameId = frameId;
        StateChanged?.Invoke();
    }

    public void ToggleRefresh()
    {
        IsRefreshPaused = !IsRefreshPaused;
        StateChanged?.Invoke();
    }
}
