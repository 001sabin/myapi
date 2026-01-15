using Serilog.Context;
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

            // 1. Generate or catch a Correlation ID
            var correlationId = Guid.NewGuid().ToString();

            // 2. LogContext.PushProperty "tags" every log entry inside this block
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString() ?? "unknown"))
            using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
            {
                try
                {
                    // Continue the pipeline
                    await _next(context);

                    stopwatch.Stop();
                    var statusCode = context.Response.StatusCode;

                    // 3. Log the completion of the request
                    // We use structured logging templates
                    var level = statusCode >= 500 ? LogLevel.Error : (statusCode >= 400 ? LogLevel.Warning : LogLevel.Information);

                    //_logger.Log(level,
                    //    "Handled {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                    //    context.Request.Method,
                    //    context.Request.Path,
                    //    statusCode,
                    //    stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex,
                        "Request {Method} {Path} failed after {ElapsedMilliseconds} ms",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds);

                    throw; // Re-throw so Global Exception handler or framework can catch it
                }
            }
        }
    }
}