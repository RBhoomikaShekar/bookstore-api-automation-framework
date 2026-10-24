namespace ApiAutomation.Framework.Config;

public class ApiSettings
{
	public string BaseUrl { get; set; } = "";
	public int TimeoutSeconds { get; set; } = 30;
	public string ClientType { get; set; } = "RestSharp";   // or "HttpClient"
	public string AdminUser { get; set; } = "admin";
	public string AdminPassword { get; set; } = "";
}