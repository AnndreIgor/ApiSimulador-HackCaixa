using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class SimulacoesQuery
{
    /// <summary>
    /// Código do produto para filtrar as simulações.
    /// </summary>
    /// <example>1</example>
    [FromQuery(Name = "codigoProduto")]
    [Range(1, int.MaxValue, ErrorMessage = "O Codigo do produto deve ser maior que {0}")]
    public int? codigoProduto { get; set; }
}
