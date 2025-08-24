namespace ApiSimulador.Contracts.Responses;

public class TelemetriaResponse
{
    /// <summary>
    /// Data de referência usada no filtro. Se não informada, será null.
    /// </summary>
    public DateOnly? DataReferencia { get; set; }

    /// <summary>
    /// Lista de métricas por endpoint monitorado.
    /// </summary>
    public List<EndpointTelemetriaDto> ListarEndpoints { get; set; } = new();
}

public class EndpointTelemetriaDto
{
    public string NomeApi { get; set; } = string.Empty;
    public int QtdRequisicoes { get; set; }
    public double TempoMedio { get; set; }
    public int TempoMinimo { get; set; }
    public int TempoMaximo { get; set; }
    public decimal PercentualSucesso { get; set; }
}
