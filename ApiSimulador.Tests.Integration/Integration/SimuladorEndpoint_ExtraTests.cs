using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ApiSimulador.Context;
using ApiSimulador.Contracts.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ApiSimulador.Tests.Integration
{
    // Reutiliza a mesma fábrica já existente no projeto
    public class SimuladorEndpoint_ExtraTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SimuladorEndpoint_ExtraTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Simular_DeveRetornar404_QuandoNaoHaProdutoQueAtendaAosParametros()
        {
            var client = _factory.CreateClient();

            // Produto seeded tem VR_MINIMO = 100 e NU_MINIMO_MESES = 1
            var req = new SimulacaoRequest { Valor = 50m, Prazo = 1 }; // abaixo do mínimo -> 404
            var resp = await client.PostAsJsonAsync("/api/v1/simulador/simular", req);

            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var json = await resp.Content.ReadAsStringAsync();
            json.Should().Contain("Nenhum produto encontrado");
        }

        [Fact]
        public async Task ResumoPorData_DeveTrazerAgregadoPorProduto_ParaDataInformada()
        {
            var client = _factory.CreateClient();

            // Gera simulações para hoje (serão salvas com DT_SIMULACAO = DateTime.Now no controller)
            var ok1 = await client.PostAsJsonAsync("/api/v1/simulador/simular", new SimulacaoRequest { Valor = 1000m, Prazo = 6 });
            var ok2 = await client.PostAsJsonAsync("/api/v1/simulador/simular", new SimulacaoRequest { Valor = 800m, Prazo = 4 });
            ok1.EnsureSuccessStatusCode();
            ok2.EnsureSuccessStatusCode();

            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var resp = await client.GetAsync($"/api/v1/simulador/simulacoes/{hoje:yyyy-MM-dd}?codigoProduto=1");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // DataReferencia == hoje
            root.GetProperty("dataReferencia").GetString().Should().Be(hoje.ToString("yyyy-MM-dd"));

            // Simulacoes: array com pelo menos 1 item do produto 1
            var sims = root.GetProperty("simulacoes");
            sims.GetArrayLength().Should().BeGreaterThan(0);

            var first = sims.EnumerateArray().First();
            first.GetProperty("codigoProduto").GetInt32().Should().Be(1);
            first.GetProperty("descricaoProduto").GetString().Should().NotBeNullOrWhiteSpace();

            // Deve trazer campos de valores totais (SAC/PRICE)
            first.TryGetProperty("valorTotalCreditoSAC", out _).Should().BeTrue();
            first.TryGetProperty("valorTotalCreditoPrice", out _).Should().BeTrue();
        }
    }
}