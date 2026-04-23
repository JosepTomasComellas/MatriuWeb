namespace MatriuWeb.Services;

public enum FrameReachability { Unknown, Ok, Slow, Unreachable, Blocked }

public interface IIframeStatusService
{
    Task<FrameReachability> CheckAsync(string url, CancellationToken ct = default);
}
