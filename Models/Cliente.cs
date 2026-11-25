using System.Text.Json.Serialization;

namespace LocadoraApi.Models;

public class Cliente : IEntidade
{
    public long Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Cnh { get; set; } = "";
    public DateOnly CnhValidade { get; set; }
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
    public DateOnly DataCadastro { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [JsonIgnore]
    public List<Locacao> Locacoes { get; } = new();

    public bool CnhEstaValida() => CnhValidade >= DateOnly.FromDateTime(DateTime.Today);

    public bool PossuiLocacoesAtrasadas() =>
        Locacoes.Any(l => l.Status == StatusLocacao.ATIVA && l.EstaAtrasada());

    public bool PodeAlugar() => CnhEstaValida() && !PossuiLocacoesAtrasadas();

    public int GetTotalLocacoes() => Locacoes.Count;
}
