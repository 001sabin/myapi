using System.Collections.Concurrent;

namespace myapi.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;

        private const int LIMIT = 5; // allow 1 request...
        private static readonly TimeSpan TIME_WINDOW = TimeSpan.FromMinutes(1); // ...per 1 minute

        // Stores: IP -> (WindowStartTime, RequestCount)
        private static readonly ConcurrentDictionary<string, RateLimitInfo> _store = new();

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1) Identify the client (IP address)
            var clientKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // 2) Current time (use UTC)
            var now = DateTime.UtcNow;

            // 3) Get the stored info for this IP OR create new if not exists
            var info = _store.GetOrAdd(clientKey, _ => new RateLimitInfo
            {
                WindowStart = now,
                Count = 0
            });

            // 4) Because many requests can come at the same time, lock this client's info
            lock (info)
            {
                // If time window passed, reset count and start a new window
                if (now - info.WindowStart > TIME_WINDOW)
                {
                    info.WindowStart = now;
                    info.Count = 0;
                }

                // Increase request count for this window
                info.Count++;

                // If exceeded limit, block request
                if (info.Count > LIMIT)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";

                    var retryAfterSeconds = (int)Math.Ceiling((TIME_WINDOW - (now - info.WindowStart)).TotalSeconds);
                    context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

                    var message = $$"""
                    {
                      "error": "Too Many Requests",
                      "message": "You are allowed {{LIMIT}} request(s) per {{(int)TIME_WINDOW.TotalSeconds}} seconds.",
                      "retryAfterSeconds": {{retryAfterSeconds}}
                    }
                    """;

                    // Stop pipeline here (DO NOT call _next)
                    context.Response.WriteAsync(message).Wait();
                    return;
                }
            }

            // 5) If not blocked, continue to next middleware/controller
            await _next(context);
        }
    }

    // A simple class that stores rate limit data for one client
    public class RateLimitInfo
    {
        public DateTime WindowStart { get; set; }
        public int Count { get; set; }
    }
}
