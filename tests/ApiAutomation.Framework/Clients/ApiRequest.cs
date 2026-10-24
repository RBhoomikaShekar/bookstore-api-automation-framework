using System.Globalization;

namespace ApiAutomation.Framework.Clients;

public sealed class ApiRequest(string resource, HttpMethod? method = null)
{
    public string Resource { get; } = resource;                       // e.g. "/books/5"
    public HttpMethod Method { get; } = method ?? HttpMethod.Get;
    public object? Body { get; private set; }
    public Dictionary<string, string> Query { get; } = new();
    public Dictionary<string, string> Headers { get; } = new();

    public ApiRequest WithBody(object body) { Body = body; return this; }
    public ApiRequest WithHeader(string key, string value) { Headers[key] = value; return this; }

    public ApiRequest WithBearer(string? token) =>
        token is null ? this : WithHeader("Authorization", $"Bearer {token}");

    public ApiRequest WithQuery(string key, object? value)
    {
        if (value is not null) Query[key] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
        return this;
    }

    public string BuildUrl() => Query.Count == 0
        ? Resource
        : $"{Resource}?{string.Join("&", Query.Select(kv =>
              $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"))}";
}