using ApiAutomation.Framework.Auth;
using ApiAutomation.Framework.Clients;
using ApiAutomation.Framework.Config;
using ApiAutomation.Framework.Helpers;
using ApiAutomation.Framework.Services;

namespace ApiAutomation.Framework;

public sealed class TestHost
{
    private static readonly Lazy<TestHost> _instance = new(() => new TestHost());
    public static TestHost Instance => _instance.Value;

    public ApiSettings Settings { get; }
    public IApiClient Client { get; }
    public AuthService Auth { get; }
    public BooksService Books { get; }
    public TokenProvider Tokens { get; }

    private TestHost()
    {
        LogConfig.Init(ConfigReader.Environment);
        Settings = ConfigReader.Settings;
        Client = ApiClientFactory.Create(Settings);
        Auth = new AuthService(Client);
        Books = new BooksService(Client);
        Tokens = new TokenProvider(Auth, Settings);

        Serilog.Log.Information("TestHost ready. Env={Env} BaseUrl={Url} Client={Client}",
            ConfigReader.Environment, Settings.BaseUrl, Settings.ClientType);
    }
}