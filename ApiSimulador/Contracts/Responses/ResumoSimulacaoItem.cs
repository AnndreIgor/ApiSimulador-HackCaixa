using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiSimulador.Contracts.Responses;

public class ResumoSimulacaoItem
{

    [JsonPropertyName("codigoProduto")]
    public int CodigoProduto { get; set; }

    [JsonPropertyName("descricaoProduto")]
    public string? DescricaoProduto { get; set; }
    [Column(TypeName = "decimal(10, 9)")]
    [JsonPropertyName("TaxaMediaJuro")]
    public decimal TaxaMediaJuro { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("valorMedioPrestacao")]
    public decimal ValorMedioPrestacao { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("ValorTotalDesejado")]
    public decimal ValorTotalDesejado { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("ValorTotalCreditoSAC")]
    public decimal ValorTotalCreditoSAC { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    [JsonPropertyName("ValorTotalCreditoPrice")]
    public decimal ValorTotalCreditoPrice { get; set; }

}
