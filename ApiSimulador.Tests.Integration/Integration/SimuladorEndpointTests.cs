using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ApiSimulador.Context;
using ApiSimulador.Contracts.Requests;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ApiSimulador.Tests.Integration;

public class SimuladorEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SimuladorEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Simular_DeveRetornarSACePRICE_EPersistirParcelas()
    {
        var client = _factory.CreateClient();

        var req = new SimulacaoRequest
        {
            Valor = 900m,   // dentro do VR_MINIMO/VR_MAXIMO seeded
            Prazo = 5       // dentro do NU_MINIMO/NU_MAXIMO
        };

        var resp = await client.PostAsJsonAsync("/api/v1/simulador/simular", req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        root.GetProperty("codigoProduto").GetInt32().Should().Be(1);
        root.GetProperty("descricaoProduto").GetString().Should().Be("Produto Teste");
        root.GetProperty("taxaJuros").GetDecimal().Should().Be(0.02m);

        // "linhas": [{ tipo:"SAC", parcelas:[...] }, { tipo:"PRICE", parcelas:[...] }]
        var linhas = root.GetProperty("linhas");
        linhas.GetArrayLength().Should().Be(2);
        var tipos = linhas.EnumerateArray().Select(e => e.GetProperty("tipo").GetString()).ToArray();
        tipos.Should().BeEquivalentTo(new[] { "SAC", "PRICE" });

        foreach (var e in linhas.EnumerateArray())
            e.GetProperty("parcelas").GetArrayLength().Should().BeGreaterThan(0);

        // Verifica persistência no MySqlDbContext (que aqui é SQLite in-memory)
        using var scope = _factory.Services.CreateScope();
        var mysql = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();

        var sims = mysql.SIMULACAO
                        .Select(s => new { s.CO_SIMULACAO, s.PZ_SIMULACAO, s.VR_SIMULACAO, Parcelas = s.Parcelas })
                        .ToList();

        sims.Should().HaveCount(1);
        sims[0].PZ_SIMULACAO.Should().Be(5);
        sims[0].VR_SIMULACAO.Should().Be(900m);
        sims[0].Parcelas.Should().NotBeNull().And.NotBeEmpty();
        sims[0].Parcelas!.Select(p => p.TP_AMORTIZACAO).Distinct()
            .Should().BeEquivalentTo(new[] { "SAC", "PRICE" });
    }

    [Fact]
    public async Task Post_Simular_ProdutoNaoEncontrado_DeveRetornar404()
    {
        var client = _factory.CreateClient();

        // Valor fora do intervalo do produto seeded
        var req = new SimulacaoRequest { Valor = 1m, Prazo = 5 };

        var resp = await client.PostAsJsonAsync("/api/v1/simulador/simular", req);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ListarSimulacoes_DeveRetornar206ComPaginacao()
    {
        var client = _factory.CreateClient();

        // Garante que exista ao menos uma simulação
        var ok = await client.PostAsJsonAsync("/api/v1/simulador/simular", new SimulacaoRequest { Valor = 900m, Prazo = 3 });
        ok.EnsureSuccessStatusCode();

        var resp = await client.GetAsync("/api/v1/simulador/simulacoes?limit=10&offset=0");
        resp.StatusCode.Should().Be((HttpStatusCode)206); // 206 Partial Content, como seu controller retorna

        var json = await resp.Content.ReadAsStringAsync();
        json.Should().Contain("\"pagina\"");
        json.Should().Contain("\"registros\"");
        json.Should().Contain("\"Links\"");
        json.Should().Contain("\"qtdRegistros\"");
    }
}