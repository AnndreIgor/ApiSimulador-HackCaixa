using ApiSimulador.Controllers;
using ApiSimulador.Models;
using FluentAssertions;
using System.Reflection;

namespace ApiSimulador.Tests.Unit.Controllers;

public class SimuladorController_CalculosTests
{
    // Helpers de reflexão para alcançar os métodos privados estáticos
    private static MethodInfo GetPrivateStatic(string name, Type[]? paramTypes = null)
    {
        var t = typeof(SimuladorController);
        var mi = t.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic, binder: null,
                             types: paramTypes, modifiers: null);
        mi.Should().NotBeNull($"Esperava encontrar método privado estático {name} em {t.FullName}");
        return mi!;
    }

    private static List<Parcela> InvokeCalcularSAC(decimal principal, int n, decimal i)
    {
        var mi = GetPrivateStatic("CalcularSAC", new[] { typeof(decimal), typeof(int), typeof(decimal) });
        var result = (List<Parcela>)mi.Invoke(null, new object[] { principal, n, i })!;
        result.Should().NotBeNull();
        return result;
    }

    private static List<Parcela> InvokeCalcularPrice(decimal principal, int n, decimal i)
    {
        var mi = GetPrivateStatic("CalcularPrice", new[] { typeof(decimal), typeof(int), typeof(decimal) });
        var result = (List<Parcela>)mi.Invoke(null, new object[] { principal, n, i })!;
        result.Should().NotBeNull();
        return result;
    }

    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    // ----------------------------
    // TESTES SAC
    // ----------------------------

    [Fact]
    public void CalcularSAC_DeveGerarParcelasEsperadasEmCasoSimples()
    {
        // Arrange
        decimal principal = 1000m;
        int n = 5;
        decimal i = 0.02m; // 2%/mês
        var amortConstante = Round2(principal / n);

        // Act
        var parcelas = InvokeCalcularSAC(principal, n, i);

        // Assert
        parcelas.Should().HaveCount(n);

        // k1: juros = 1000*0.02 = 20.00 | amort=200.00 | parcela=220.00
        parcelas[0].NU_PARCELA.Should().Be(1);
        parcelas[0].VR_JUROS.Should().Be(Round2(1000m * i)); // 20.00
        parcelas[0].VR_AMORTIZACAO.Should().Be(amortConstante); // 200.00
        parcelas[0].VR_PRESTACAO.Should().Be(Round2(parcelas[0].VR_JUROS + parcelas[0].VR_AMORTIZACAO)); // 220.00

        // k2: saldo 800 → juros 16.00 | amort=200.00 | prest=216.00
        parcelas[1].NU_PARCELA.Should().Be(2);
        parcelas[1].VR_JUROS.Should().Be(16.00m);
        parcelas[1].VR_AMORTIZACAO.Should().Be(200.00m);
        parcelas[1].VR_PRESTACAO.Should().Be(216.00m);

        // k5: saldo antes = 200 → juros 4.00 | amort deve ajustar = 200.00 | prest=204.00
        parcelas[4].NU_PARCELA.Should().Be(5);
        parcelas[4].VR_JUROS.Should().Be(4.00m);
        parcelas[4].VR_AMORTIZACAO.Should().Be(200.00m);
        parcelas[4].VR_PRESTACAO.Should().Be(204.00m);

        // Soma das amortizações = principal
        parcelas.Sum(p => p.VR_AMORTIZACAO).Should().Be(principal);
    }

    [Fact]
    public void CalcularSAC_DeveAjustarUltimaParcelaParaZerarSaldo()
    {
        // Arrange
        decimal principal = 1000m;
        int n = 3;
        decimal i = 0.02m;

        // Act
        var parcelas = InvokeCalcularSAC(principal, n, i);

        // Assert
        parcelas.Should().HaveCount(3);
        // Reconstruir saldo
        var saldo = principal;
        for (int k = 0; k < n; k++)
            saldo = Round2(saldo - parcelas[k].VR_AMORTIZACAO);

        saldo.Should().Be(0.00m); // saldo zera no fim
    }


    [Fact]
    public void CalcularPrice_DeveUsarPMTCorretaEAteZerarSaldo()
    {
        // Arrange
        decimal principal = 1000m;
        int n = 5;
        decimal i = 0.02m;

        // PMT teórica arredondada como no código (Round2 no PMT)
        var fator = (decimal)Math.Pow((double)(1 + i), -n);
        var pmtRaw = principal * i / (1 - fator);
        var pmt = Round2(pmtRaw);

        // Act
        var parcelas = InvokeCalcularPrice(principal, n, i);

        // Assert básicos
        parcelas.Should().HaveCount(n);
        parcelas.All(p => p.VR_PRESTACAO == pmt).Should().BeTrue("PRICE mantém prestação fixa (após o Round2 aplicado ao PMT)");

        // Reconstruir saldo e conferir consistência juros/amortização
        var saldo = principal;
        for (int k = 0; k < n; k++)
        {
            var jurosExp = Round2(saldo * i);
            parcelas[k].VR_JUROS.Should().Be(jurosExp);
            parcelas[k].VR_AMORTIZACAO.Should().Be(Round2(pmt - jurosExp));

            saldo = Round2(saldo - parcelas[k].VR_AMORTIZACAO);
        }

        saldo.Should().Be(0.00m); // saldo final zera
    }

    [Fact]
    public void Round2_DeveSerAwayFromZero()
    {
        // Apenas para documentar o comportamento de arredondamento do sistema
        Round2(2.345m).Should().Be(2.35m);   // 5 sobe
        Round2(-2.345m).Should().Be(-2.35m); // longe do zero
        Round2(2.344m).Should().Be(2.34m);
        Round2(-2.344m).Should().Be(-2.34m);
    }

}
