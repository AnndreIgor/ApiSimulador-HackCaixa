using ApiSimulador.Models;
using ApiSimulador.Context;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var started = DateTimeOffset.UtcNow;
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var ended = DateTimeOffset.UtcNow;

            // Tentar capturar o "nome/padrão" do endpoint (rota)
            string? routePattern = null;
            if (context.GetEndpoint() is RouteEndpoint re)
                routePattern = re.RoutePattern.RawText;
            else
                routePattern = context.GetEndpoint()?.DisplayName;

            // Grava no banco criando um escopo
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();

            var log = new RequestLog
            {
                Method = context.Request.Method,
                Path = context.Request.Path.ToString(),
                Route = routePattern,
                StartedAtUtc = started,
                EndedAtUtc = ended,
                StatusCode = context.Response.StatusCode,
                DurationMs = (int)sw.ElapsedMilliseconds
            };

            await db.RequestLogs.AddAsync(log);
            await db.SaveChangesAsync();
        }
    }
}
