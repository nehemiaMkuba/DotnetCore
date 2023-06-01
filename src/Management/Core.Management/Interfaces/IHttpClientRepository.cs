using System.Threading.Tasks;
using System.Collections.Generic;

using Core.Domain.Enums;

namespace Core.Management.Interfaces
{

    public interface IHttpClientRepository
    {
        Task<TResult> GetAsync<TResult>(string uri, Dictionary<string, string> headers, Dictionary<string, string> queryStrings, string[]? routeParameters = null);
        Task<TResult> PostAsync<TResult>(string uri, Dictionary<string, string> headers, string payload, RequestContentType requestContentType);
        Task<TResult> PutAsync<TRequest, TResult>(string uri, Dictionary<string, string> headers, TRequest payload, RequestContentType requestContentType);
    }
}