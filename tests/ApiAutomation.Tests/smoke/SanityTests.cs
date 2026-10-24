using System.Net;
using ApiAutomation.Framework.Builders;
using ApiAutomation.Framework.Clients;
using Shouldly;

namespace ApiAutomation.Tests.Smoke;

public class SanityTests : TestBase
{
    [Test, Category("Smoke")]
    public async Task Health_ReturnsOk()
    {
        Report.Info("GET /health");
        var res = await Host.Client.SendAsync<object>(new ApiRequest("/health"));

        res.StatusCode.ShouldBe(HttpStatusCode.OK, res.ErrorMessage);
    }

    [Test, Category("Smoke")]
    public async Task CreateThenGetBook_RoundTrips()
    {
        var token = await Host.Tokens.GetAdminTokenAsync();
        var request = new BookBuilder().Build();

        Report.Info($"Creating a book with ISBN {request.Isbn}");
        var created = await Host.Books.CreateAsync(request, token);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        Track(created.Data!.Id);                       // auto-deleted in TearDown

        Report.Info($"Fetching book {created.Data.Id}");
        var fetched = await Host.Books.GetAsync(created.Data.Id);
        fetched.StatusCode.ShouldBe(HttpStatusCode.OK);
        fetched.Data!.Isbn.ShouldBe(request.Isbn);
        fetched.Data.Title.ShouldBe(request.Title);
    }
}