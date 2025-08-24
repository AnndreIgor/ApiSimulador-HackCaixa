using System.Text.Json.Serialization;

namespace ApiSimulador.Contracts.Responses
{
    public class ResumoSimulacoesResponse
    {
        /// <summary>
        /// Data de referencia das simulacoes.
        /// </summary>
        /// <example>900</example>
        [JsonPropertyName("dataReferencia")]
        public DateOnly DataReferencia { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        /// <summary>
        /// Data de referencia das simulacoes.
        /// </summary>
        /// <example>900</example>
        [JsonPropertyName("simulacoes")]
        public List<ResumoSimulacaoItem> Simulacoes { get; set; } = new();
    }
}