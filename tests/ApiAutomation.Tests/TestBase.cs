using System.Net;
using ApiAutomation.Framework;
using ApiAutomation.Framework.Reporting;
using AventStack.ExtentReports;
using NUnit.Framework.Interfaces;

namespace ApiAutomation.Tests;

public abstract class TestBase
{
    protected static TestHost Host => TestHost.Instance;

    // Use inside tests to write steps into the report: Report.Info("Created book 5");
    protected ExtentTest Report { get; private set; } = default!;

    private readonly List<int> _createdBookIds = [];

    // Register a book so it is deleted automatically after the test
    protected void Track(int bookId) => _createdBookIds.Add(bookId);

    [SetUp]
    public void StartReport()
    {
        var test = TestContext.CurrentContext.Test;
        Report = ExtentManager.Instance.CreateTest(test.Name);

        var categories = test.Properties["Category"].Cast<string>().ToArray();
        if (categories.Length > 0) Report.AssignCategory(categories);
    }

    [TearDown]
    public async Task EndTest()
    {
        // 1) record pass / fail in the report
        var result = TestContext.CurrentContext.Result;
        switch (result.Outcome.Status)
        {
            case TestStatus.Passed:
                Report.Pass("Test passed");
                break;
            case TestStatus.Failed:
                Report.Fail($"<pre>{WebUtility.HtmlEncode(result.Message)}\n{WebUtility.HtmlEncode(result.StackTrace)}</pre>");
                break;
            default:
                Report.Skip(result.Message ?? "Skipped");
                break;
        }

        // 2) delete test data, even when the test failed
        if (_createdBookIds.Count == 0) return;
        try
        {
            var token = await Host.Tokens.GetAdminTokenAsync();
            foreach (var id in _createdBookIds)
                await Host.Books.DeleteAsync(id, token);
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "Cleanup failed for books {Ids}", _createdBookIds);
        }
        finally { _createdBookIds.Clear(); }
    }
}