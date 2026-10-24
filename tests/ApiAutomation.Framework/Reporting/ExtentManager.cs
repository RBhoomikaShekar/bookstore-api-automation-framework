using ApiAutomation.Framework.Config;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;

namespace ApiAutomation.Framework.Reporting;

public static class ExtentManager
{
    private static readonly Lazy<ExtentReports> _extent = new(Create);

    public static ExtentReports Instance => _extent.Value;
    public static string ReportDirectory => Path.Combine(AppContext.BaseDirectory, "reports");
    public static string ReportPath => Path.Combine(ReportDirectory, "index.html");

    private static ExtentReports Create()
    {
        Directory.CreateDirectory(ReportDirectory);

        var spark = new ExtentSparkReporter(ReportPath);
        spark.Config.DocumentTitle = "BookStore API Test Report";
        spark.Config.ReportName = "BookStore API Automation";
        spark.Config.Theme = Theme.Standard;
        spark.Config.OfflineMode = true;       // bundle CSS/JS so the file works anywhere

        var extent = new ExtentReports();
        extent.AttachReporter(spark);
        extent.AddSystemInfo("Environment", ConfigReader.Environment);
        extent.AddSystemInfo("Base URL", ConfigReader.Settings.BaseUrl);
        extent.AddSystemInfo("HTTP client", ConfigReader.Settings.ClientType);
        extent.AddSystemInfo(".NET", System.Environment.Version.ToString());
        return extent;
    }

    // Writes the HTML file. Call once, at the very end of the run.
    public static void Flush()
    {
        if (_extent.IsValueCreated) _extent.Value.Flush();
    }
}