using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Munk.AspNetCore.Ex;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceStack;

namespace Munk.AspNetCore
{
    public class JsonExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly TelemetryClient _telemetry;

        public JsonExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment, TelemetryClient telemetry)
        {
            _next = next;
            _hostingEnvironment = hostingEnvironment;
            _telemetry = telemetry;
        }

        public async Task Invoke(HttpContext context)
        {
            Exception exception = null;

            try
            {
                await _next.Invoke(context);
            }
            catch (WebServiceException ex) //Pass along exception from call to other service
            {
                Debug.WriteLine(ex);
                _telemetry.TrackTrace("JsonExceptionHandlingMiddleware.WebServiceException returning exception.responseBody: " + ex.ResponseBody);
                _telemetry.TrackException(ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; 
                await context.Response.WriteAsync(ex.ResponseBody);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (ex is BaseException { SkipExceptionLoggingInMiddleware: true })
                {
                    _telemetry.TrackTrace("Skipped logging of exception: " + ex);
                }
                else
                {
                    _telemetry.TrackException(ex);
                }
                exception = ex;
            }
            context.Request.EnableBuffering();

            if (!context.Response.HasStarted && exception != null)
            {
                ApiErrorResponse response;

                //if (_hostingEnvironment.IsProduction())
                //{
                    response = new ApiErrorResponse(context.Response.StatusCode, exception.Message, exception.ToString());

                //else                //}
                //{
                //    response = new ApiErrorResponse(context.Response.StatusCode, userMessage + ". " + exception.Message, exception.ToString());
                //}
                
                var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(json);
            }
        }
    }

    public static class JsonExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JsonExceptionHandlingMiddleware>();
        }
    }

}
