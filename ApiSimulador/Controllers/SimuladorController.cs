using ApiSimulador.Context;
using ApiSimulador.Contracts.Requests;
using ApiSimulador.Contracts.Responses;
using ApiSimulador.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;


namespace ApiSimulador.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiConventionType(typeof(DefaultApiConventions))]

    public class SimuladorController : ControllerBase
    {
        private readonly MySqlDbContext _mysqlContext;
        private readonly SqlServerDbContext _sqlServerContext;

        public SimuladorController(MySqlDbContext mysqlContext, SqlServerDbContext sqlServerContext)
        {
            _mysqlContext = mysqlContext;
            _sqlServerContext = sqlServerContext;
        }

        /// <summary>
        /// Faz a simulação de credito nas tabelas SAC e PRICE.
        /// </summary>
        [HttpPost("simular")]
        public async Task<ActionResult> Simular([FromBody] SimulacaoRequest req)
        {
            try
            {
                if (req == null)
                    return BadRequest("Requisição inválida.");

                var produto = await _sqlServerContext.PRODUTO
                    .FirstOrDefaultAsync(p =>
                        req.Valor >= p.VR_MINIMO &&
                        req.Valor <= p.VR_MAXIMO &&
                        req.Prazo >= p.NU_MINIMO_MESES &&
                        req.Prazo <= p.NU_MAXIMO_MESES);

                if (produto == null)
                    return NotFound(new { Mensagem = "Nenhum produto encontrado para os parâmetros fornecidos." });

                if (produto.PC_TAXA_JUROS <= 0) 
                    throw new ArgumentException("Taxa de juros deve ser positiva para PRICE");

                var linhasPrice = CalcularPrice(req.Valor, req.Prazo, produto.PC_TAXA_JUROS);
                var linhasSac = CalcularSAC(req.Valor, req.Prazo, produto.PC_TAXA_JUROS);

                var simu = new Simulacao
                {
                    PC_TAXA_JUROS = produto.PC_TAXA_JUROS,
                    PZ_SIMULACAO = req.Prazo,
                    VR_SIMULACAO = req.Valor,
                    CO_PRODUTO = produto.CO_PRODUTO
                };

                await _mysqlContext.SIMULACAO.AddAsync(simu);
                await _mysqlContext.SaveChangesAsync();

                var entidades = new List<Parcela>();
                entidades.AddRange(linhasPrice.Select(x => new Parcela
                {
                    TP_AMORTIZACAO = "PRICE",
                    NU_PARCELA = x.NU_PARCELA,
                    VR_PRESTACAO = x.VR_PRESTACAO,
                    VR_JUROS = x.VR_JUROS,
                    VR_AMORTIZACAO = x.VR_AMORTIZACAO,
                    Simulacao = simu
                }));

                entidades.AddRange(linhasSac.Select(x => new Parcela
                {
                    TP_AMORTIZACAO = "SAC",
                    NU_PARCELA = x.NU_PARCELA,
                    VR_PRESTACAO = x.VR_PRESTACAO,
                    VR_JUROS = x.VR_JUROS,
                    VR_AMORTIZACAO = x.VR_AMORTIZACAO,
                    Simulacao = simu
                }));

                await _mysqlContext.PARCELA.AddRangeAsync(entidades);
                await _mysqlContext.SaveChangesAsync();

                return Ok(new
                {
                    codigoProduto = produto.CO_PRODUTO,
                    descricaoProduto = produto.NO_PRODUTO,
                    taxaJuros = produto.PC_TAXA_JUROS,
                    linhas = new[]
                    {
                        new { tipo = "SAC", parcelas = linhasSac },
                        new { tipo = "PRICE", parcelas = linhasPrice }
                    }
                });
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao processar simulação: {ex.Message}", statusCode: 500);
            }
        }

        /// <summary>
        /// Lista todas as simulações realizadas.
        /// </summary>
        /// <remarks>
        /// Esse endpoint retorna uma lista de simulações existentes no sistema.
        /// 
        /// Exemplos de uso:
        /// 
        /// - `GET /api/simulacoes`
        /// </remarks>
        /// <response code="206">Retorna uma página da lista de simulações</response>
        /// <response code="400">Se os parâmetros de filtro forem inválidos</response>
        /// <response code="500">Se ocorrer um erro interno no servidor</response>
        [HttpGet("simulacoes")]
        public async Task<ActionResult> listarSimulacoes([FromQuery] ListaSimulacaoQuery q)
        {
            try
            {
                var total = await _mysqlContext.SIMULACAO.AsNoTracking().CountAsync();

                var simulacoes = await _mysqlContext.SIMULACAO
                    .OrderByDescending(s => s.DT_SIMULACAO)
                    .Include(s => s.Parcelas)
                    .AsNoTracking()
                    .Skip(q.offset)
                    .Take(q.limit)
                    .Select(s => new
                    {
                        idSimulacao = s.CO_SIMULACAO,
                        valorDesejado = s.VR_SIMULACAO,
                        prazo = s.PZ_SIMULACAO,
                        s.TotalPrestacaoSAC,
                        s.TotalPrestacaoPRICE
                    })
                    .ToListAsync();

                if (!simulacoes.Any())
                    return NoContent();

                int totalPages = (int)Math.Ceiling(total / (double)q.limit);
                int currentPage = (q.offset / q.limit) + 1;

                string baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
                string first = $"{baseUrl}?limit={q.limit}&offset=0";
                string last = $"{baseUrl}?limit={q.limit}&offset={(totalPages - 1) * q.limit}";
                string? next = (q.offset + q.limit < total) ? $"{baseUrl}?limit={q.limit}&offset={q.offset + q.limit}" : null;
                string? prev = (q.offset > 0) ? $"{baseUrl}?limit={q.limit}&offset={Math.Max(0, q.offset - q.limit)}" : null;

                return StatusCode(StatusCodes.Status206PartialContent, new
                {
                    pagina = currentPage,
                    qtdRegistros = total,
                    qtdRegistrosPagina = q.limit,
                    registros = simulacoes,
                    Links = new[] { first, next, prev, last }.Where(x => x != null)
                });
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao listar simulações: {ex.Message}", statusCode: 500);
            }
        }

        // GET /api/simulacoes/2025-08-21?codigoProduto=123
        /// <summary>
        /// Lista os valores simulados em uma data, agregados pelo código do produto.
        /// </summary>
        /// <remarks>
        /// Esse endpoint retorna uma sumarização das simulações realizadas.
        /// 
        /// Exemplos de uso:
        /// 
        /// - `GET /api/simulacoes/2025-08-20` -> retorna todas as simulações do dia 20 de agosto de 2025.
        /// - `GET /api/simulacoes/2025-08-20?codigoProduto=1` -> retorna apenas as simulações do produto 1.
        /// 
        /// O formato da data segue o padrão (`yyyy-MM-dd`).
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos de simulação correspondentes aos filtros informados.
        /// </returns>
        /// <response code="200">Retorna a lista de simulações</response>
        /// <response code="400">Se os parâmetros de filtro forem inválidos</response>
        /// <response code="500">Se ocorrer um erro interno no servidor</response>
        [HttpGet("simulacoes/{data-referencia}")]
        public IActionResult Get(
            [FromRoute(Name = "data-referencia")] DateOnly dataReferencia, 
            [FromQuery] SimulacoesQuery query)
        {
            
            var querySimulacoes = _mysqlContext.SIMULACAO.AsNoTracking();
            querySimulacoes = querySimulacoes.Where(s => s.DT_SIMULACAO.Date == dataReferencia.ToDateTime(TimeOnly.MinValue).Date);

            if (query.codigoProduto.HasValue) 
            {
                querySimulacoes = querySimulacoes.Where(s => s.CO_PRODUTO == query.codigoProduto);
            }

            var descricoes = _sqlServerContext.PRODUTO
                .AsNoTracking()
                .Select(p => new { p.CO_PRODUTO, p.NO_PRODUTO })
                .ToDictionary(x => x.CO_PRODUTO, x => x.NO_PRODUTO);

            var itens = querySimulacoes
                .Include(s => s.Parcelas)
                .ToList()
                .GroupBy(s => s.CO_PRODUTO)
                .Select(g => new ResumoSimulacaoItem
                {
                    CodigoProduto = g.Key,
                    DescricaoProduto = descricoes.TryGetValue(g.Key, out var nome) ? nome : null,
                    TaxaMediaJuro = g.Average(x => x.PC_TAXA_JUROS),
                    ValorTotalDesejado = Math.Round(g.Sum(x => x.VR_SIMULACAO), 2),
                    ValorMedioPrestacao = g.SelectMany(x => x.Parcelas).Any()
                        ? g.SelectMany(x => x.Parcelas).Average(p => p.VR_PRESTACAO)
                        : 0m,
                    ValorTotalCreditoSAC = g.SelectMany(x => x.Parcelas)
                                            .Where(p => p.TP_AMORTIZACAO.Equals("SAC", StringComparison.OrdinalIgnoreCase))
                                            .Sum(p => p.VR_PRESTACAO),
                    ValorTotalCreditoPrice = g.SelectMany(x => x.Parcelas)
                                              .Where(p => p.TP_AMORTIZACAO.Equals("PRICE", StringComparison.OrdinalIgnoreCase))
                                              .Sum(p => p.VR_PRESTACAO)
                })
                .ToList();

            var resposta = new ResumoSimulacoesResponse
            {
                DataReferencia = dataReferencia,
                Simulacoes = itens
            };

            return Ok(resposta);
        }

        /// <summary>
        /// Obtém dados de telemetria da API de simulação.
        /// </summary>
        /// <remarks>
        /// Essa rota retorna estatísticas sobre as requisições (exceto chamadas à rota `/simular`):
        ///
        /// - **nomeApi**: nome da API monitorada  
        /// - **quantidadeRequisicoes**: total de requisições registradas  
        /// - **tempoMedio**: tempo médio de resposta em milissegundos  
        /// - **tempoMinimo**: menor tempo de resposta em milissegundos  
        /// - **tempoMaximo**: maior tempo de resposta em milissegundos  
        /// - **percentualSucesso**: percentual de respostas com status HTTP 2xx  
        /// </remarks>
        /// <response code="200">Retorna as estatísticas de telemetria calculadas</response>
        [HttpGet("telemetria")]
        [ProducesResponseType(typeof(TelemetriaResponse), StatusCodes.Status200OK)]
        public IActionResult Telemetria([FromQuery] TelemetriaQuery query)
        {
            var logs = _mysqlContext.RequestLogs
                .AsNoTracking()
                .Where(r => r.Route != null && r.Route.EndsWith("/simular"));

            if (query.DataReferencia.HasValue)
            {
                var data = query.DataReferencia.Value;
                var inicio = data.ToDateTime(TimeOnly.MinValue);
                var fim = data.AddDays(1).ToDateTime(TimeOnly.MinValue);

                logs = logs.Where(r => r.StartedAtUtc >= inicio && r.StartedAtUtc < fim);
            }

            var lista = logs.ToList();

            var total = lista.Count;
            var tempoMedio = total == 0 ? 0.0 : lista.Average(l => (double)l.DurationMs);
            var tempoMin = total == 0 ? 0 : lista.Min(l => l.DurationMs);
            var tempoMax = total == 0 ? 0 : lista.Max(l => l.DurationMs);
            var sucesso = total == 0 ? 0 : lista.Count(l => l.StatusCode >= 200 && l.StatusCode < 300);
            var percentual = total == 0 ? 0m : Math.Round((decimal)sucesso * 100m / total, 2);

            var response = new TelemetriaResponse
            {
                DataReferencia = query.DataReferencia,
                ListarEndpoints = new List<EndpointTelemetriaDto>
        {
            new EndpointTelemetriaDto
            {
                NomeApi = "Simulacao",
                QtdRequisicoes = total,
                TempoMedio = tempoMedio,
                TempoMinimo = tempoMin,
                TempoMaximo = tempoMax,
                PercentualSucesso = percentual
            }
        }
            };

            return Ok(response);
        }


        private static List<Parcela> CalcularPrice(decimal principal, int n, decimal i)
        {
            // PMT = P * i / (1 - (1 + i)^(-n))
            decimal fator = (decimal)Math.Pow((double)(1 + i), -n);
            decimal pmt = principal * i / (1 - fator);
            pmt = Round2(pmt);

            var linhas = new List<Parcela>(n);
            decimal saldo = principal;

            for (short k = 1; k <= n; k++)
            {
                decimal juros = Round2(saldo * i);
                decimal amort = Round2(pmt - juros);

                if (k == n)
                    amort = Round2(saldo);

                saldo = Round2(saldo - amort);

                linhas.Add(new Parcela
                {
                    NU_PARCELA = k,
                    VR_PRESTACAO = pmt,
                    VR_JUROS = juros,
                    VR_AMORTIZACAO = amort
                });
            }

            return (linhas);
        }

        private static List<Parcela> CalcularSAC(decimal principal, int n, decimal i)
        {
            var linhas = new List<Parcela>(n);
            decimal amortConstante = Round2(principal / n);
            decimal saldo = principal;

            for (short k = 1; k <= n; k++)
            {
                decimal juros = Round2(saldo * i);
                decimal amort = (k == n) ? Round2(saldo) : amortConstante;
                decimal parcela = Round2(amort + juros);

                saldo = Round2(saldo - amort);

                linhas.Add(new Parcela
                {
                    NU_PARCELA = k,
                    VR_PRESTACAO = parcela,
                    VR_JUROS = juros,
                    VR_AMORTIZACAO = amort
                });
            }

            return linhas;
        }

        private static decimal Round2(decimal v) =>
            Math.Round(v, 2, MidpointRounding.AwayFromZero);

    }
}
