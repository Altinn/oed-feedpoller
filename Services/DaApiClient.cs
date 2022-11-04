using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using oed_feedpoller.Interfaces;

namespace oed_feedpoller.Services;
public class DaApiClient : IDaApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache _distributedCache;

    public DaApiClient(IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
    {
        _httpClientFactory = httpClientFactory;
        _distributedCache = distributedCache;
    }

    public async Task<T> GetCachedAsync<T>(string url) where T : class
    {
        var result = await GetCachedAsync(url);
        return JsonSerializer.Deserialize<T>(result)!;
    }

    public async Task<string> GetCachedAsync(string url)
    {
        var key = "GET_" + url;
        var result = await _distributedCache.GetStringAsync(key);
        if (result != null) return result;

        var client = _httpClientFactory.CreateClient(Constants.DaHttpClient);
        result = await client.GetStringAsync(url);
        await _distributedCache.SetAsync(key, Encoding.UTF8.GetBytes(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(60)
        });

        return result;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var client = _httpClientFactory.CreateClient(Constants.DaHttpClient);
        return await client.SendAsync(request);
    }
}
