using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiSimulador.Models;

public class Simulacao
{
    public Simulacao()
    {
        Parcelas = new List<Parcela>();
    }
    [Key]
    [JsonPropertyName("idSimulacao")]
    public int CO_SIMULACAO { get; set; }
    [Required]
    [Column(TypeName = "decimal(10, 9)")]
    [JsonIgnore]
    public decimal PC_TAXA_JUROS { get; set; }
    [Required]
    [JsonPropertyName("prazo")]
    public short PZ_SIMULACAO { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("valorDesejado")]
    public decimal VR_SIMULACAO { get; set; }
    [JsonIgnore]
    public DateTime DT_SIMULACAO { get; set; } = DateTime.Now;
    [JsonIgnore]
    public int CO_PRODUTO { get; set; }
    //[JsonIgnore]
    public List<Parcela> Parcelas { get; set; }
    [NotMapped]
    [JsonPropertyName("valorTotalParcelasSAC")]
    public decimal TotalPrestacaoSAC =>
    Parcelas?
        .Where(p => string.Equals(p.TP_AMORTIZACAO, "SAC", StringComparison.OrdinalIgnoreCase))
        .Sum(p => p.VR_PRESTACAO) ?? 0m;

    [NotMapped]
    [JsonPropertyName("valorTotalParcelasPRICE")]
    public decimal TotalPrestacaoPRICE =>
    Parcelas?
        .Where(p => string.Equals(p.TP_AMORTIZACAO, "PRICE", StringComparison.OrdinalIgnoreCase))
        .Sum(p => p.VR_PRESTACAO) ?? 0m;

}
