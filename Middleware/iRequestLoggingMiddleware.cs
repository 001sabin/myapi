using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace myapi.Middleware
{
    public class iRequestLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        // No RequestDelegate in constructor for IMiddleware
        public iRequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        // IMiddleware signature includes `next`
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();

            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString.Value;

            try
            {
                // continue pipeline
                await next(context);

                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "HTTP {Method} {Path}{QueryString} threw an exception after {ElapsedMilliseconds}ms",
                    method, path, queryString, stopwatch.ElapsedMilliseconds);

                throw; // rethrow so outer handlers can handle it
            }
        }
    }
}
