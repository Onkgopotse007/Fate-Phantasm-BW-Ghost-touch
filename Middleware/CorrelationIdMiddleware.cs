using Serilog.Context;

namespace RPG_dotnet.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValues)
                ? headerValues.FirstOrDefault() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            context.Items["correlationId"] = correlationId;

            context.Response.Headers[CorrelationIdHeader] = correlationId;

            using (LogContext.PushProperty("correlationId", correlationId))
            using (LogContext.PushProperty("clientIpAddress", context.Connection.RemoteIpAddress?.ToString()))
            using (LogContext.PushProperty("requestMethod", context.Request.Method))
            using (LogContext.PushProperty("requestPath", context.Request.Path.ToString()))
            using (LogContext.PushProperty("userAgent", context.Request.Headers["User-Agent"].ToString()))
            {
                await _next(context);
            }
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}

