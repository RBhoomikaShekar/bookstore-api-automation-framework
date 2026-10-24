using System.Text.Json;
using Serilog;
using Serilog.Events;

namespace ApiAutomation.Framework.Clients;

public sealed class LoggingApiClient(IApiClient inner) : IApiClient
{
    public async Task<ApiResponse<T>> SendAsync<T>(ApiRequest request)
    {
        var url = request.BuildUrl();
        var sensitive = request.Resource.StartsWith("/auth", StringComparison.OrdinalIgnoreCase);

        Log.Debug("--> {Method} {Url} body={Body}", request.Method, url,
                  sensitive ? "[REDACTED]" : JsonSerializer.Serialize(request.Body));

        var res = await inner.SendAsync<T>(request);       // do the real work

        Log.Write(res.IsSuccessful ? LogEventLevel.Information : LogEventLevel.Warning,
                  "<-- {Status} {Method} {Url} in {Ms}ms {Error}",
                  (int)res.StatusCode, request.Method, url,
                  (long)res.ElapsedTime.TotalMilliseconds, res.ErrorMessage);

        if (!sensitive) Log.Debug("    response body: {Body}", Truncate(res.RawContent, 2000));
        return res;
    }

    private static string? Truncate(string? s, int max) =>
        s is null || s.Length <= max ? s : s[..max] + "…";
}