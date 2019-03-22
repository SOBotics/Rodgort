using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Rodgort
{
    // https://stackoverflow.com/a/38935583/563532
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var httpStatusCode = (int)HttpStatusCode.InternalServerError;
            if (exception is HttpStatusException requestException)
                httpStatusCode = (int) requestException.StatusCode;

            if (httpStatusCode >= 400 && httpStatusCode < 500)
                _logger.LogWarning(exception, $"{httpStatusCode} ({context.Connection.RemoteIpAddress}) - {context.Request?.Path.Value}");
            else
                _logger.LogError(exception, $"{context.Request?.Path.Value} - {httpStatusCode}");

            var result = JsonConvert.SerializeObject(new ErrorWrapper { Error = exception.Message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = httpStatusCode;
            return context.Response.WriteAsync(result);
        }
    }

    public class ErrorWrapper
    {
        public string Error { get; set; }
    }

    public class HttpStatusException : Exception
    {
        public HttpStatusException(HttpStatusCode statusCode, string message = null)
            : base(message)
        {
            StatusCode = statusCode;
        }
        public HttpStatusException(int statusCode, string message = null)
            : this((HttpStatusCode)statusCode, message)
        {

        }

        public HttpStatusCode StatusCode { get; }
    }
}
