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
    /// <summary>
    /// Endpoint da API.
    /// </summary>
    /// <example>simulacao</example>
    public string NomeApi { get; set; } = string.Empty;
    /// <summary>
    /// Quantidade de requisições recebidas no endpoint.
    /// </summary>
    /// <example>50</example>
    public int QtdRequisicoes { get; set; }
    /// <summary>
    /// Tempo medio de resposta.
    /// </summary>
    /// <example>200.78</example>
    public double TempoMedio { get; set; }
    /// <summary>
    /// Tempo mínimo de resposta.
    /// </summary>
    /// <example>50.02</example>
    public int TempoMinimo { get; set; }
    /// <summary>
    /// Tempo máximo de resposta.
    /// </summary>
    /// <example>500.45</example>
    public int TempoMaximo { get; set; }
    /// <summary>
    /// Percentual de requisições atendidas com sucesso.
    /// </summary>
    /// <example>93.78</example>
    public decimal PercentualSucesso { get; set; }
}
