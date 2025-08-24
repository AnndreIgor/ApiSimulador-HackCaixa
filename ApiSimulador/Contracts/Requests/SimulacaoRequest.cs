using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSimulador.Contracts.Requests
{
    [NotMapped]
    public class SimulacaoRequest
    {
        /// <summary>
        /// Valor do empréstimo.
        /// </summary>
        /// <example>900</example>
        [Required(ErrorMessage = "O valor do emprestimo é obrigatório!")]
        [Range(0.1, 7922816251426433, ErrorMessage = "O valor do emprestimo deve ser maior que zero")]
        public decimal Valor { get; set; }  // ex.: valor do empréstimo

        /// <summary>
        /// Prazo do contrato em meses.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "O numero de parcelas é obrigatório!")]
        [Range(1, 420, ErrorMessage = "O valor deve estar entre zero e 420.")]
        public short Prazo { get; set; }      // ex.: prazo em meses
    }
}