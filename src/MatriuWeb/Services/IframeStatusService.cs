using System.Diagnostics;

namespace MatriuWeb.Services;

public class IframeStatusService : IIframeStatusService
{
    private readonly IHttpClientFactory _http;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<IframeStatusService> _log;

    private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan _slowThreshold = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public IframeStatusService(IHttpClientFactory http, IRedisCacheService cache, ILogger<IframeStatusService> log)
    {
        _http = http;
        _cache = cache;
        _log = log;
    }

    public async Task<FrameReachability> CheckAsync(string url, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _)) return FrameReachability.Unknown;

        var cacheKey = $"frame:status:{url}";
        var cached = await _cache.GetAsync<FrameReachability>(cacheKey);
        if (cached != FrameReachability.Unknown) return cached;

        var result = await ProbeAsync(url, ct);
        await _cache.SetAsync(cacheKey, result, _cacheTtl);
        return result;
    }

    private async Task<FrameReachability> ProbeAsync(string url, CancellationToken ct)
    {
        using var client = _http.CreateClient("iframe-probe");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        var sw = Stopwatch.StartNew();
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, url);
            req.Headers.TryAddWithoutValidation("User-Agent", "MatriuWeb/1.0 iframe-probe");

            using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            sw.Stop();

            if (IsBlockedByHeaders(resp))
                return FrameReachability.Blocked;

            return sw.Elapsed > _slowThreshold ? FrameReachability.Slow : FrameReachability.Ok;
        }
        catch (OperationCanceledException)
        {
            return FrameReachability.Slow;
        }
        catch (Exception ex)
        {
            _log.LogDebug("Probe failed for {Url}: {Msg}", url, ex.Message);
            return FrameReachability.Unreachable;
        }
    }

    private static bool IsBlockedByHeaders(HttpResponseMessage resp)
    {
        if (resp.Headers.TryGetValues("X-Frame-Options", out var xfo))
        {
            var v = string.Join(",", xfo).ToUpperInvariant();
            if (v.Contains("DENY") || v.Contains("SAMEORIGIN")) return true;
        }

        if (resp.Headers.TryGetValues("Content-Security-Policy", out var csp))
        {
            var v = string.Join(" ", csp);
            if (v.Contains("frame-ancestors 'none'")) return true;
        }

        return false;
    }
}
