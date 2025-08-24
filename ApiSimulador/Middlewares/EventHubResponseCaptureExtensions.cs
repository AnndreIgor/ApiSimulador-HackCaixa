namespace ApiSimulador.Middlewares;

public static class EventHubResponseCaptureExtensions
{
    public static IApplicationBuilder UseEventHubResponseCapture(this IApplicationBuilder app)
        => app.UseMiddleware<EventHubResponseCaptureMiddleware>();
}