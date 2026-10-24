namespace ApiAutomation.Framework.Clients;

public interface IApiClient
{
	Task<ApiResponse<T>> SendAsync<T>(ApiRequest request);
}
