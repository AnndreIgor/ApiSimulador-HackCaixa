using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSimulador.Models;

public class Produto
{
    [Key]   
    public int CO_PRODUTO { get; set; }
    [Required]
    public string NO_PRODUTO { get; set; } = string.Empty;
    [Required]
    [Column(TypeName = "decimal(10, 9)")]
    public decimal PC_TAXA_JUROS { get; set; }
    [Required]
    public short NU_MINIMO_MESES { get; set; } 
    public short? NU_MAXIMO_MESES { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? VR_MINIMO { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? VR_MAXIMO { get; set; }
}
