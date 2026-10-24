using ApiAutomation.Framework.Config;

namespace ApiAutomation.Framework.Clients;

public static class ApiClientFactory
{
    public static IApiClient Create(ApiSettings settings)
    {
        IApiClient core = settings.ClientType.Equals("HttpClient", StringComparison.OrdinalIgnoreCase)
            ? new HttpClientApiClient(settings)
            : new RestSharpApiClient(settings);

        // Retry on the outside, logging inside: every single attempt gets logged.
        return new RetryingApiClient(new LoggingApiClient(core));
    }
}