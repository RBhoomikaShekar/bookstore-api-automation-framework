using System.Net;
using System.Text.Json;

namespace ApiAutomation.Framework.Clients;

public sealed class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; init; }
    public bool IsSuccessful => (int)StatusCode is >= 200 and < 300;
    public T? Data { get; init; }                 // the JSON converted to an object (only for 2xx)
    public string? RawContent { get; init; }      // the JSON as text
    public string? ContentType { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
    public TimeSpan ElapsedTime { get; init; }
    public string? ErrorMessage { get; init; }    // timeout, connection refused, bad JSON...

    // Convert the raw body to any other type, e.g. an error body: res.ReadAs<ProblemDetailsDto>()
    public TResult? ReadAs<TResult>() =>
        string.IsNullOrWhiteSpace(RawContent)
            ? default
            : JsonSerializer.Deserialize<TResult>(RawContent, JsonDefaults.Options);

    public static ApiResponse<T> From(HttpStatusCode status, string? raw, string? contentType,
        IEnumerable<KeyValuePair<string, string>> headers, TimeSpan elapsed, string? error = null)
    {
        T? data = default;
        if ((int)status is >= 200 and < 300 && !string.IsNullOrWhiteSpace(raw) && typeof(T) != typeof(object))
        {
            try { data = JsonSerializer.Deserialize<T>(raw, JsonDefaults.Options); }
            catch (JsonException ex) { error = $"Deserialization failed: {ex.Message}"; }
        }

        return new ApiResponse<T>
        {
            StatusCode = status,
            Data = data,
            RawContent = raw,
            ContentType = contentType,
            ElapsedTime = elapsed,
            ErrorMessage = error,
            Headers = headers
                .GroupBy(h => h.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => string.Join(",", g.Select(x => x.Value)),
                              StringComparer.OrdinalIgnoreCase)
        };
    }
}