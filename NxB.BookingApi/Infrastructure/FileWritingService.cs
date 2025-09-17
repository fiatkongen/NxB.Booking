using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using ServiceStack.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NxB.BookingApi.Infrastructure
{
    public static class DebugFileHelper
    {
        public static void WriteToFile(string content, string contextName)
        {
            var dataFolderPath = Path.Combine("C:\\temp", "Data");

            // Create the data folder if it doesn't exist
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }

            var fileName = $"{DateTime.Now:yyyy-MM-dd HH-mm-ss} {contextName}.txt";
            var filePath = Path.Combine(dataFolderPath, fileName);

            // Write to the file
            using (var writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(content);
            }
        }
    }

    public class LoggingHandler : DelegatingHandler
    {
        private readonly string _contextName;

        public LoggingHandler(HttpMessageHandler innerHandler, string contextName)
            : base(innerHandler)
        {
            _contextName = contextName;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log request content before sending
            await LogRequest(request);

            // Call the inner handler
            var response = await base.SendAsync(request, cancellationToken);

            // Log response content after receiving
            string responseContent = $"Response Http code: {response.StatusCode}\n"; ;
            if (request.Content != null)
            {
                responseContent += BuildJson(await response.Content.ReadAsStringAsync());
            }

            DebugFileHelper.WriteToFile(responseContent, "2 Response " + _contextName);

            return response;
        }

        private async Task LogRequest(HttpRequestMessage request)
        {
            var log = $"Request Method: {request.Method}\nRequest URI: {request.RequestUri}\n";

            // Log headers
            foreach (var header in request.Headers)
            {
                log += $"Header: {header.Key} - {string.Join(", ", header.Value)}\n";
            }

            // Log content if present
            if (request.Content != null)
            {
                log += $"Request Content: {BuildJson(await request.Content.ReadAsStringAsync())}\n";
            }

            DebugFileHelper.WriteToFile(log, "1 Request " + _contextName);
        }

        private string BuildJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";
            JToken token = JToken.Parse(json);

            // Serialize the JToken to JSON with indented formatting
            return token.ToString(Formatting.Indented);
        }
    }
}