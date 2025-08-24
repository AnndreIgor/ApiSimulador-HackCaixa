namespace ApiSimulador.Models;

public class RequestLog
{
    public int Id { get; set; }
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public string? Route { get; set; } // nome/padrão da rota, se disponível
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset EndedAtUtc { get; set; }
    public int StatusCode { get; set; }
    public int DurationMs { get; set; }
}
