using Polly;
using Polly.Retry;
using Serilog;

namespace ApiAutomation.Framework.Clients;

public sealed class RetryingApiClient(IApiClient inner, int maxRetries = 2) : IApiClient
{
    // Safe to repeat without side effects:
    private static readonly HashSet<string> Idempotent = ["GET", "HEAD", "PUT", "DELETE", "OPTIONS"];

    public async Task<ApiResponse<T>> SendAsync<T>(ApiRequest request)
    {
        // POST is not safe to repeat: a retry could create a duplicate book
        if (!Idempotent.Contains(request.Method.Method))
            return await inner.SendAsync<T>(request);

        var pipeline = new ResiliencePipelineBuilder<ApiResponse<T>>()
            .AddRetry(new RetryStrategyOptions<ApiResponse<T>>
            {
                MaxRetryAttempts = maxRetries,
                Delay = TimeSpan.FromMilliseconds(300),
                BackoffType = DelayBackoffType.Exponential,       // wait 300ms, then 600ms...
                ShouldHandle = new PredicateBuilder<ApiResponse<T>>()
                    .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == 0),
                OnRetry = args =>
                {
                    Log.Warning("Retry #{Attempt} for {Method} {Resource}",
                                args.AttemptNumber + 1, request.Method, request.Resource);
                    return default;
                }
            })
            .Build();

        return await pipeline.ExecuteAsync(async _ => await inner.SendAsync<T>(request));
    }
}