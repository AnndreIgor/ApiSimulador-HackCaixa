namespace ApiSimulador.Contracts.Requests;

public class TelemetriaQuery
{
    /// <summary>
    /// Data de referência para filtrar as requisições (opcional).
    /// - Se informada, retorna apenas as requisições do dia especificado.
    /// - Se não informada, retorna todos os registros do período disponível.
    /// O formato da data segue o padrão (`yyyy-MM-dd`).
    /// </summary>
    public DateOnly? DataReferencia { get; set; }
}