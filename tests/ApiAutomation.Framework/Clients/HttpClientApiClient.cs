using System.Diagnostics;
using System.Net.Http.Json;
using ApiAutomation.Framework.Config;

namespace ApiAutomation.Framework.Clients;

public sealed class HttpClientApiClient : IApiClient, IDisposable
{
    private readonly HttpClient _http;

    public HttpClientApiClient(ApiSettings settings)
    {
        // Create ONE HttpClient and reuse it. PooledConnectionLifetime refreshes connections
        // every 2 minutes, which avoids both classic HttpClient problems (explained below).
        var handler = new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(2) };
        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri(settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        };
    }

    public async Task<ApiResponse<T>> SendAsync<T>(ApiRequest request)
    {
        using var message = new HttpRequestMessage(request.Method, request.BuildUrl());
        foreach (var (key, value) in request.Headers) message.Headers.TryAddWithoutValidation(key, value);
        if (request.Body is not null)
            message.Content = JsonContent.Create(request.Body, request.Body.GetType(),
                                                 options: JsonDefaults.Options);

        var sw = Stopwatch.StartNew();
        try
        {
            using var res = await _http.SendAsync(message);
            var raw = await res.Content.ReadAsStringAsync();
            sw.Stop();

            var headers = res.Headers.Concat(res.Content.Headers)
                .Select(h => new KeyValuePair<string, string>(h.Key, string.Join(",", h.Value)));

            return ApiResponse<T>.From(res.StatusCode, raw,
                res.Content.Headers.ContentType?.MediaType, headers, sw.Elapsed);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            sw.Stop();
            return ApiResponse<T>.From(0, null, null, [], sw.Elapsed, ex.Message);  // 0 = no HTTP response
        }
    }

    public void Dispose() => _http.Dispose();
}
