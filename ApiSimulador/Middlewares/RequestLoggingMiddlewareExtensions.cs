public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestDbLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}