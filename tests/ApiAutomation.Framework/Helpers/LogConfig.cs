using Serilog;

namespace ApiAutomation.Framework.Helpers;

public static class LogConfig
{
    private static int _initialized;

    public static void Init(string environment)
    {
        // Run only once, even if many tests call this at the same time
        if (Interlocked.Exchange(ref _initialized, 1) == 1) return;

        const string template =
            "{Timestamp:HH:mm:ss.fff} [{Level:u3}] [{Env}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithProperty("Env", environment)
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "api-tests-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: template,
                shared: true)
            .CreateLogger();
    }
}