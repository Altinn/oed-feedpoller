using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oed_feedpoller.Interfaces;
public interface IDaApiClient
{
    public Task<T> GetCachedAsync<T>(string url) where T : class;
    public Task<string> GetCachedAsync(string url);
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}
