using ApiAutomation.Framework.Config;
using ApiAutomation.Framework.Services;

namespace ApiAutomation.Framework.Auth;

public sealed class TokenProvider(AuthService auth, ApiSettings settings)
{
    // Lazy<Task<string>>: the first caller triggers the login; everyone else awaits the SAME login.
    // Result: exactly one login per test run, even with parallel tests.
    private readonly Lazy<Task<string>> _admin =
        new(() => auth.LoginAndGetTokenAsync(settings.AdminUser, settings.AdminPassword));

    public Task<string> GetAdminTokenAsync() => _admin.Value;

    // Registers a brand-new ordinary user and returns its token. Used for 403 (forbidden) tests.
    public async Task<string> CreateUserTokenAsync()
    {
        var name = "u_" + Guid.NewGuid().ToString("N")[..12];
        const string password = "Passw0rd!";

        var reg = await auth.RegisterAsync(name, password);
        if (!reg.IsSuccessful)
            throw new InvalidOperationException($"Register failed: {reg.RawContent}");

        return await auth.LoginAndGetTokenAsync(name, password);
    }
}