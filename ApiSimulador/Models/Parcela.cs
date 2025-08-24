using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiSimulador.Models;

public class Parcela
{
    [Key]
    [JsonIgnore]
    public int CO_PARCELA { get; set; }
    [Required]
    [JsonPropertyName("numero")]
    public short NU_PARCELA { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("valorAmortizacao")]
    public decimal VR_AMORTIZACAO { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("valorJuros")]
    public decimal VR_JUROS { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("valorPrestacao")]
    public decimal VR_PRESTACAO { get; set; }
    [Required]
    [JsonIgnore]
    public string TP_AMORTIZACAO { get; set; } = string.Empty;
    [JsonIgnore]
    public int CO_SIMULACAO { get; set; }
    [JsonIgnore]
    public Simulacao Simulacao { get; set; }
}
