using ApiAutomation.Framework.Clients;
using ApiAutomation.Framework.Models;

namespace ApiAutomation.Framework.Services;

public class BooksService(IApiClient client)
{
    public Task<ApiResponse<BookDto>> CreateAsync(BookRequest body, string? token = null) =>
        CreateRawAsync(body, token);

    // Accepts ANY object so negative tests can send wrong types, e.g. new { price = "abc" }
    public Task<ApiResponse<BookDto>> CreateRawAsync(object body, string? token = null) =>
        client.SendAsync<BookDto>(new ApiRequest("/books", HttpMethod.Post).WithBody(body).WithBearer(token));

    public Task<ApiResponse<BookDto>> GetAsync(int id) =>
        client.SendAsync<BookDto>(new ApiRequest($"/books/{id}"));

    public Task<ApiResponse<PagedResult<BookDto>>> ListAsync(int? page = null, int? pageSize = null,
        string? search = null, string? sortBy = null, bool? desc = null) =>
        client.SendAsync<PagedResult<BookDto>>(new ApiRequest("/books")
            .WithQuery("page", page).WithQuery("pageSize", pageSize)
            .WithQuery("search", search).WithQuery("sortBy", sortBy)
            .WithQuery("desc", desc?.ToString().ToLowerInvariant()));

    public Task<ApiResponse<BookDto>> UpdateAsync(int id, BookRequest body, string? token = null) =>
        client.SendAsync<BookDto>(new ApiRequest($"/books/{id}", HttpMethod.Put).WithBody(body).WithBearer(token));

    public Task<ApiResponse<object>> DeleteAsync(int id, string? token = null) =>
        client.SendAsync<object>(new ApiRequest($"/books/{id}", HttpMethod.Delete).WithBearer(token));
}