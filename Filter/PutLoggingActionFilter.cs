using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace myapi.Filter
{
    public class PutLoggingActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<PutLoggingActionFilter> _logger;

        public PutLoggingActionFilter(ILogger<PutLoggingActionFilter> logger)
        {
            _logger = logger;
        }



        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Only log PUT requests
            if (!HttpMethods.IsPut(context.HttpContext.Request.Method))
            {
                await next(); // continue without logging
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path.Value;

            // Action info (filter-only advantage)
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            // Execute the action (controller method)
            var executedContext = await next();

            stopwatch.Stop();

            var statusCode = context.HttpContext.Response.StatusCode;

            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
            {
                _logger.LogError(executedContext.Exception,
                    "PUT Action FAILED: {Controller}.{Action} {Method} {Path} in {ElapsedMs} ms",
                    controllerName, actionName, method, path, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "PUT Action: {Controller}.{Action} {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    controllerName, actionName, method, path, statusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
