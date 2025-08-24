using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class SimulacoesQuery
{
    /// <summary>
    /// Código do produto para filtrar as simulações.
    /// </summary>
    /// <example>1</example>
    [FromQuery(Name = "codigoProduto")]
    [Range(1, int.MaxValue)]
    public int? codigoProduto { get; set; }
}
