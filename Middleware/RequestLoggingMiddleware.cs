using System.Diagnostics;

namespace myapi.Middleware
{

  
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var method = context.Request.Method;

            var path = context.Request.Path;

            var queryString = context.Request.QueryString.Value;  //.HasValue ? context.Request.QueryString.Value : string.Empty;


           try {
                await _next(context);
                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 500)
                {
                    _logger.LogError("HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning("HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMilliseconds}ms",
                        method, path, queryString, statusCode, stopwatch.ElapsedMilliseconds);
                }
                
            } catch(Exception ex) {

                stopwatch.Stop();

                _logger.LogError(ex, "HTTP {Method} {Path}{QueryString} threw an exception after {ElapsedMilliseconds}ms",
                    method, path, queryString, stopwatch.ElapsedMilliseconds);

                throw;// yeslai throw garera feri universal or next middleware ko handler ma pathako
            }

        }
    }
}
