using System.ComponentModel.DataAnnotations;

namespace ApiSimulador.Contracts.Requests
{
    public class ListaSimulacaoQuery
    {
        /// <summary>
        /// Quantidade de simulações por página.
        /// </summary>
        /// <example>50</example>
        [Range(1, 200, ErrorMessage = "A quantidade de simulações por página deve estar entre {1} e {100}")]
        public int limit { get; set; } = 50;

        /// <summary>
        /// Offset para paginação, ou seja, quantas simulações devem ser puladas antes de começar a listar.
        /// </summary>
        /// <example>0</example>
        [Range(0, int.MaxValue, ErrorMessage = "O offset não pode ser negativo")]
        public int offset { get; set; } = 0;
    }
}
