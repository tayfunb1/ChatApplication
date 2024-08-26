using System.Diagnostics;

namespace ChatApplication.Middlewares;

public class RequestTrackingMiddleware(RequestDelegate next, ILogger<RequestTrackingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await next(context);
        
        stopwatch.Stop();
        
        var elapsed = stopwatch.ElapsedMilliseconds;
        logger.LogInformation($"Request [{context.Request.Method}] to [{context.Request.Path}] took {elapsed} ms");
    }
}