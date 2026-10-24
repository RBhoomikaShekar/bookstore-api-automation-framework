using ApiAutomation.Framework.Clients;
using ApiAutomation.Framework.Models;

namespace ApiAutomation.Framework.Services;

public class AuthService(IApiClient client)
{
    public Task<ApiResponse<object>> RegisterAsync(string username, string password) =>
        client.SendAsync<object>(new ApiRequest("/auth/register", HttpMethod.Post)
            .WithBody(new { username, password }));

    public Task<ApiResponse<TokenDto>> LoginAsync(string username, string password) =>
        client.SendAsync<TokenDto>(new ApiRequest("/auth/login", HttpMethod.Post)
            .WithBody(new { username, password }));

    // Convenience: log in and return just the token string, or fail with a clear message
    public async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var res = await LoginAsync(username, password);
        if (!res.IsSuccessful || res.Data is null)
            throw new InvalidOperationException(
                $"Login failed for '{username}': {(int)res.StatusCode} {res.RawContent} {res.ErrorMessage}");
        return res.Data.AccessToken;
    }
}