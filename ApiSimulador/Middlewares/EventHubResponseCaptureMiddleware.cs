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
    private readonly HashSet<string> _targets; // rotas a monitorar (opcional)

    public EventHubResponseCaptureMiddleware(
        RequestDelegate next,
        EventHubProducerClient producer,
        IOptions<EventHubOptions> options,
        ILogger<EventHubResponseCaptureMiddleware> logger)
    {
        _next = next;
        _producer = producer;
        _logger = logger;
        _targets = new HashSet<string>(options.Value.TargetRoutes ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // só captura JSON e (opcionalmente) rotas específicas
        // se não tiver lista configurada, captura tudo que for JSON
        bool isTargetRoute = _targets.Count == 0 || _targets.Contains(context.Request.Path);
        if (!isTargetRoute)
        {
            await _next(context);
            return;
        }

        // trocamos o stream da resposta por um buffer temporário
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context); // deixa a pipeline processar e escrever no buffer

            // só envia se o content-type indicar JSON e houver corpo
            var contentType = context.Response.ContentType ?? "";
            bool isJson = contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
            buffer.Position = 0;

            if (isJson && buffer.Length > 0)
            {
                using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                string bodyText = await reader.ReadToEndAsync();

                // volta o ponteiro para copiar ao cliente
                buffer.Position = 0;

                // envia de forma resiliente, mas sem derrubar a resposta ao cliente se falhar
                _ = SendToEventHubSafeAsync(context, bodyText);
            }

            // copia o buffer de volta para o response real
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
            // cuidado com limite: Event Hubs ~1 MB por evento
            var data = new EventData(Encoding.UTF8.GetBytes(json));

            // (opcional) chave de partição para “juntar” eventos por rota
            var opts = new SendEventOptions
            {
                PartitionKey = ctx.Request.Path.HasValue ? ctx.Request.Path.Value : "unknown"
            };

            await _producer.SendAsync(new[] { data }, opts);
        }
        catch (Exception ex)
        {
            // não propaga erro para o cliente — apenas loga
            _logger.LogError(ex, "Falha ao enviar resposta ao Event Hub para {Path}", ctx.Request.Path);
        }
    }
}
