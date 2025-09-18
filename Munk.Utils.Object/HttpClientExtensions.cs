using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace System.Net.Http
{
    // from https://gist.github.com/alexandrevicenzi/9216739
    public static class HttpClientEx
    {
        public const string MimeJson = "application/json";

        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(client.BaseAddress + requestUri),
                Content = content,
            };

            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync(this HttpClient client, string requestUri, Type type, object value)
        {
            var serializedDoc = JsonConvert.SerializeObject(value);
            var requestContent = new StringContent(serializedDoc, Encoding.UTF8, "application/json-patch+json");
            return client.PutAsync(requestUri, requestContent);
        }

        public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var serializedDoc = JsonConvert.SerializeObject(value);
            var requestContent = new StringContent(serializedDoc, Encoding.UTF8, "application/json-patch+json");
            return client.PatchAsync(requestUri, requestContent);
        }
    }
}

