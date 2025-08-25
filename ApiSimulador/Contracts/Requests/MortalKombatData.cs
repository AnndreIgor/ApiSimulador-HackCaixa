using ApiSimulador.Controllers;

namespace ApiSimulador.Contracts.Requests;

public class MortalKombatData
{
    public string Jogo { get; set; }
    public string Notacao { get; set; }
    public List<string> Observacoes { get; set; }
    public List<Brutality> Brutalities { get; set; }
}