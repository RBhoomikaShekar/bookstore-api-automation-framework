using System.Diagnostics;
using System.Text.Json;
using ApiAutomation.Framework.Config;
using RestSharp;

namespace ApiAutomation.Framework.Clients;

public sealed class RestSharpApiClient : IApiClient, IDisposable
{
    private readonly RestClient _client;

    public RestSharpApiClient(ApiSettings settings) =>
        _client = new RestClient(new RestClientOptions(settings.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds)
        });

    public async Task<ApiResponse<T>> SendAsync<T>(ApiRequest request)
    {
        // 1) translate our neutral ApiRequest into RestSharp's request
        var rq = new RestRequest(request.Resource,
                                 Enum.Parse<Method>(request.Method.Method, ignoreCase: true));
        foreach (var (key, value) in request.Query) rq.AddQueryParameter(key, value);
        foreach (var (key, value) in request.Headers) rq.AddOrUpdateHeader(key, value);
        if (request.Body is not null)
            rq.AddStringBody(JsonSerializer.Serialize(request.Body, JsonDefaults.Options), DataFormat.Json);

        // 2) send it and time it
        var sw = Stopwatch.StartNew();
        var res = await _client.ExecuteAsync(rq);     // does NOT throw on 400/404/500
        sw.Stop();

        // 3) translate RestSharp's response into our neutral ApiResponse
        var headers = (res.Headers ?? []).Concat(res.ContentHeaders ?? [])
            .Select(h => new KeyValuePair<string, string>(h.Name, h.Value ?? ""));

        return ApiResponse<T>.From(res.StatusCode, res.Content,
            res.ContentType?.Split(';')[0].Trim(), headers, sw.Elapsed, res.ErrorMessage);
    }

    public void Dispose() => _client.Dispose();
}