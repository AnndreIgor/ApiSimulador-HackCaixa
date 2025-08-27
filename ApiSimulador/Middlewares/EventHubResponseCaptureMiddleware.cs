namespace ApiSimulador.Middlewares;

using ApiSimulador.Options;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;
using System.Text;

public class EventHubResponseCaptureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EventHubProducerClient _producer;
    private readonly ILogger<EventHubResponseCaptureMiddleware> _logger;
    private readonly HashSet<string> _targets;

    public EventHubResponseCaptureMiddleware(
        RequestDelegate next,
        EventHubProducerClient producer,
        IOptions<EventHubOptions> options,
        ILogger<EventHubResponseCaptureMiddleware> logger)
    {
        _next = next;
        _producer = producer;
        _logger = logger;
        _targets = new HashSet<string>(
            options.Value.TargetRoutes ?? new List<string>(),
            StringComparer.OrdinalIgnoreCase
        );
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool isTargetRoute =
            _targets.Count > 0 &&
            _targets.Contains(context.Request.Path.Value ?? string.Empty);

 

        if (!isTargetRoute)
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            bool isOk = context.Response.StatusCode == StatusCodes.Status200OK;
            var contentType = context.Response.ContentType ?? string.Empty;
            bool isJson = contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);

            buffer.Position = 0;

            if (isOk && isJson && buffer.Length > 0)
            {
                using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                string bodyText = await reader.ReadToEndAsync();
                buffer.Position = 0;

                _ = SendToEventHubSafeAsync(context, bodyText);
            }

            await buffer.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private async Task SendToEventHubSafeAsync(HttpContext ctx, string json)
    {
        try
        {
            var data = new EventData(Encoding.UTF8.GetBytes(json));

            var opts = new SendEventOptions
            {
                PartitionKey = ctx.Request.Path.HasValue ? ctx.Request.Path.Value : "unknown"
            };

            await _producer.SendAsync(new[] { data }, opts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar resposta ao Event Hub para {Path}", ctx.Request.Path);
        }
    }
}
