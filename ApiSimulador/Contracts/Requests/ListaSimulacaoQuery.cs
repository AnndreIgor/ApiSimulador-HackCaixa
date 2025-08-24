using System.ComponentModel.DataAnnotations;

namespace ApiSimulador.Contracts.Requests
{
    public class ListaSimulacaoQuery
    {
        /// <summary>
        /// Quantidade de simulações por página.
        /// </summary>
        /// <example>50</example>
        [Range(1, 50)]
        public int limit { get; set; } = 50;

        /// <summary>
        /// Offset para paginação, ou seja, quantas simulações devem ser puladas antes de começar a listar.
        /// </summary>
        /// <example>0</example>
        [Range(0, int.MaxValue)]
        public int offset { get; set; } = 0;
    }
}
