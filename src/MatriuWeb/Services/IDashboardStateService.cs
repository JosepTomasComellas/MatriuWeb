namespace MatriuWeb.Services;

public interface IDashboardStateService
{
    string? FocusedFrameId { get; }
    bool IsRefreshPaused { get; }
    event Action? StateChanged;
    void FocusFrame(string? frameId);
    void ToggleRefresh();
}
