using System.Text.Json;

namespace ApiAutomation.Framework.Clients;

public static class JsonDefaults
{
    // "Web" defaults: camelCase names (accessToken) and case-insensitive reading
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}