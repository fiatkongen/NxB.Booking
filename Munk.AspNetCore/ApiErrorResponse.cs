using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Munk.AspNetCore
{
    //https://www.devtrends.co.uk/blog/handling-errors-in-asp.net-core-web-api
    public class ApiErrorResponse
    {
        public int StatusCode { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }
        public string DetailedErrorMessage { get; set; }

        public ApiErrorResponse(int statusCode, string errorMessage = null, string detailedErrorMessage = null)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            DetailedErrorMessage = detailedErrorMessage;
        }
    }

    public class ApiErrorBadRequestResponse : ApiErrorResponse
    {
        public List<string> Errors { get; }

        public ApiErrorBadRequestResponse() : base(400, "Bad request")
        {

        }

        public ApiErrorBadRequestResponse(string message) : base(400, message)
        {
        }

        public ApiErrorBadRequestResponse(ModelStateDictionary modelState) : base(400)
        {
            if (modelState.IsValid)
            {
                throw new ArgumentException("ModelState must be invalid", nameof(modelState));
            }

            Errors = modelState.SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage).ToList();

            ErrorMessage = Errors.First();
            DetailedErrorMessage = string.Join("\n", Errors);
        }
    }
}