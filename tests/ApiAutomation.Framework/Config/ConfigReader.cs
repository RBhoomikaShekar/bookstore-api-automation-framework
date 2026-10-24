using Microsoft.Extensions.Configuration;

namespace ApiAutomation.Framework.Config;

public static class ConfigReader
{
    private static readonly Lazy<ApiSettings> _settings = new(Load);

    public static ApiSettings Settings => _settings.Value;

    // Which environment? Read from the TEST_ENV variable; default is "local".
    public static string Environment =>
        System.Environment.GetEnvironmentVariable("TEST_ENV") ?? "local";

    private static ApiSettings Load()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)                      // the folder where tests run (bin/...)
            .AddJsonFile("appsettings.json", optional: false)           // must exist
            .AddJsonFile($"appsettings.{Environment}.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)      // your private overrides
            .AddEnvironmentVariables()                                  // ApiSettings__BaseUrl=...
            .Build();

        var settings = config.GetSection("ApiSettings").Get<ApiSettings>()
                       ?? throw new InvalidOperationException("ApiSettings section missing.");

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            throw new InvalidOperationException("ApiSettings:BaseUrl is empty.");

        return settings;
    }
}