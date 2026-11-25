using System.Text.Json.Serialization;

namespace LocadoraApi.Models;

public class Funcionario : IEntidade
{
    private const decimal PercentualComissao = 0.02m; // 2% do faturamento

    public long Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Matricula { get; set; } = "";
    public string Cargo { get; set; } = "";
    public decimal Salario { get; set; }
    public DateOnly DataAdmissao { get; set; }
    public bool Ativo { get; set; } = true;

    [JsonIgnore]
    public List<Locacao> Locacoes { get; } = new();

    public bool EstaAtivo() => Ativo;

    /// <summary>Valida as regras e cria a locação (orquestra Cliente + Veiculo).</summary>
    public Locacao RegistrarLocacao(Cliente cliente, Veiculo veiculo, int dias)
    {
        if (!EstaAtivo())
            throw new RegraDeNegocioException("Funcionário inativo não pode registrar locações.");
        if (dias <= 0)
            throw new RegraDeNegocioException("A quantidade de dias deve ser maior que zero.");
        if (!cliente.CnhEstaValida())
            throw new RegraDeNegocioException("Cliente com CNH vencida não pode alugar.");
        if (cliente.PossuiLocacoesAtrasadas())
            throw new RegraDeNegocioException("Cliente possui locações atrasadas.");
        if (!veiculo.EstaDisponivel())
            throw new RegraDeNegocioException("Veículo não está disponível para locação.");

        var agora = DateTime.Now;
        var locacao = new Locacao
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            FuncionarioId = Id,
            DataRetirada = agora,
            DataDevolucaoPrevista = agora.AddDays(dias),
            KmRetirada = veiculo.Quilometragem,
            ValorDiaria = veiculo.ValorDiaria, // snapshot do valor
            ValorTotal = veiculo.CalcularValorLocacao(dias),
            Status = StatusLocacao.ATIVA,
            Cliente = cliente,
            Veiculo = veiculo,
            Funcionario = this
        };

        veiculo.MarcarComoLocado();
        veiculo.Locacoes.Add(locacao);
        cliente.Locacoes.Add(locacao);
        Locacoes.Add(locacao);
        return locacao;
    }

    public int GetTotalLocacoesRealizadas(int mes, int ano) =>
        Locacoes.Count(l => l.Status != StatusLocacao.CANCELADA
                            && l.DataRetirada.Month == mes && l.DataRetirada.Year == ano);

    public decimal CalcularComissao(int mes, int ano)
    {
        var faturamento = Locacoes
            .Where(l => l.Status == StatusLocacao.FINALIZADA
                        && l.DataRetirada.Month == mes && l.DataRetirada.Year == ano)
            .Sum(l => l.ValorTotal);
        return Math.Round(faturamento * PercentualComissao, 2);
    }
}
