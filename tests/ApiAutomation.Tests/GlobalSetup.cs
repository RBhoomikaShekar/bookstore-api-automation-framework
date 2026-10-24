using ApiAutomation.Framework;
using ApiAutomation.Framework.Reporting;

namespace ApiAutomation.Tests;

[SetUpFixture]    // covers every test in this namespace and the ones below it (Smoke, Regression...)
public class GlobalSetup
{
    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _ = TestHost.Instance;   // load config and logging now, so a configuration mistake fails immediately
    }

    [OneTimeTearDown]
    public void AfterAllTests()
    {
        ExtentManager.Flush();   // write the HTML report
        Serilog.Log.Information("Report written to {Path}", ExtentManager.ReportPath);
    }
}