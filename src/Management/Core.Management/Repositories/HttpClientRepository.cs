using System.IO;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.WebUtilities;

using Core.Domain.Enums;
using Core.Management.Interfaces;

namespace Core.Management.Repositories
{

    public class HttpClientRepository : IHttpClientRepository
    {
        private readonly IHttpClientFactory _clientFactory;

        public HttpClientRepository(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<TResult> GetAsync<TResult>(string uri, Dictionary<string, string> headers, Dictionary<string, string> queryStrings, string[]? routeParameters = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(uri);

            if (routeParameters != null && routeParameters.Length > 0)
            {
                for (int i = 0; i < routeParameters.Length; i++)
                {
                    stringBuilder.Append($"/{routeParameters[i]}");
                }
            }

            string url = stringBuilder.ToString();

            if (queryStrings != null && queryStrings.Count > 0) url = QueryHelpers.AddQueryString(url, queryStrings);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            HttpClient client = _clientFactory.CreateClient();

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync
            <TResult>(responseStream))!;

        }

        public async Task<TResult> PostAsync<TResult>(string uri, Dictionary<string, string> headers, string payload, RequestContentType requestContentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            switch (requestContentType)
            {
                case RequestContentType.FormUrlEncodedContent:

                    Dictionary<string, string>? postData = JsonSerializer.Deserialize<Dictionary<string, string>>(payload);
                    if (postData != null) request.Content = new FormUrlEncodedContent(postData);

                    break;
                case RequestContentType.StringContent:

                    request.Content = new StringContent(payload, Encoding.UTF8,
                                    "application/json");
                    break;
                default:
                    break;
            }


            HttpClient client = _clientFactory.CreateClient();

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync
            <TResult>(responseStream))!;
        }

        public async Task<TResult> PutAsync<TRequest, TResult>(string uri, Dictionary<string, string> headers, TRequest payload, RequestContentType requestContentType)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, uri);
            string jsonRequest = JsonSerializer.Serialize(payload);

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            switch (requestContentType)
            {
                case RequestContentType.FormUrlEncodedContent:

                    Dictionary<string, string>? postData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonRequest);
                    if (postData != null) request.Content = new FormUrlEncodedContent(postData);

                    break;
                case RequestContentType.StringContent:

                    request.Content = new StringContent(jsonRequest, Encoding.UTF8,
                                   "application/json");
                    break;
                default:
                    break;
            }

            HttpClient client = _clientFactory.CreateClient();

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return (await JsonSerializer.DeserializeAsync
            <TResult>(responseStream))!;
        }
    }
}