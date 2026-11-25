using System.Text.Json.Serialization;

namespace LocadoraApi.Models;

public class Locacao : IEntidade
{
    private const decimal PercentualMultaAtraso = 0.10m; // 10% da diária por dia

    public long Id { get; set; }
    public long ClienteId { get; set; }
    public long VeiculoId { get; set; }
    public long FuncionarioId { get; set; }
    public DateTime DataRetirada { get; set; }
    public DateTime DataDevolucaoPrevista { get; set; }
    public DateTime? DataDevolucaoReal { get; set; }
    public double KmRetirada { get; set; }
    public double? KmDevolucao { get; set; }
    public decimal ValorDiaria { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusLocacao Status { get; set; } = StatusLocacao.ATIVA;
    public List<Multa> Multas { get; } = new();

    [JsonIgnore] public Cliente? Cliente { get; set; }
    [JsonIgnore] public Veiculo? Veiculo { get; set; }
    [JsonIgnore] public Funcionario? Funcionario { get; set; }

    public int DiasContratados =>
        Math.Max(1, (int)Math.Ceiling((DataDevolucaoPrevista - DataRetirada).TotalDays));

    public bool EstaAtrasada()
    {
        if (Status == StatusLocacao.CANCELADA) return false;
        var referencia = DataDevolucaoReal ?? DateTime.Now;
        return referencia > DataDevolucaoPrevista;
    }

    public int CalcularDiasAtraso()
    {
        if (!EstaAtrasada()) return 0;
        var referencia = DataDevolucaoReal ?? DateTime.Now;
        return (int)Math.Ceiling((referencia - DataDevolucaoPrevista).TotalDays);
    }

    public decimal CalcularMultaAtraso() =>
        CalcularDiasAtraso() * ValorDiaria * PercentualMultaAtraso;

    /// <param name="dataDevolucao">Opcional: permite simular devoluções (útil para testar atraso).</param>
    public void Finalizar(double kmDevolucao, DateTime? dataDevolucao = null)
    {
        if (Status != StatusLocacao.ATIVA)
            throw new RegraDeNegocioException("Apenas locações ativas podem ser finalizadas.");
        if (kmDevolucao < KmRetirada)
            throw new RegraDeNegocioException("A quilometragem de devolução não pode ser menor que a de retirada.");

        var devolucao = dataDevolucao ?? DateTime.Now;
        if (devolucao < DataRetirada)
            throw new RegraDeNegocioException("A data de devolução não pode ser anterior à retirada.");

        DataDevolucaoReal = devolucao;
        KmDevolucao = kmDevolucao;

        var multaAtraso = CalcularMultaAtraso();
        if (multaAtraso > 0)
        {
            var multa = new Multa
            {
                LocacaoId = Id,
                Tipo = TipoMulta.ATRASO,
                Descricao = $"Atraso de {CalcularDiasAtraso()} dia(s)",
                Valor = multaAtraso
            };
            multa.Aplicar();
            Multas.Add(multa);
        }

        Status = StatusLocacao.FINALIZADA;
        ValorTotal = CalcularValorFinal();
        Veiculo?.RegistrarQuilometragem(kmDevolucao);
        Veiculo?.LiberarParaLocacao();
    }

    public void Cancelar()
    {
        if (Status != StatusLocacao.ATIVA)
            throw new RegraDeNegocioException("Apenas locações ativas podem ser canceladas.");
        Status = StatusLocacao.CANCELADA;
        Veiculo?.LiberarParaLocacao();
    }

    public decimal CalcularValorFinal() =>
        DiasContratados * ValorDiaria + Multas.Sum(m => m.Valor);

    /// <summary>Adiciona uma multa manual (DANO, MULTA_TRANSITO...) e atualiza o total se já finalizada.</summary>
    public Multa AdicionarMulta(TipoMulta tipo, string descricao, decimal valor)
    {
        if (Status == StatusLocacao.CANCELADA)
            throw new RegraDeNegocioException("Não é possível aplicar multa em locação cancelada.");
        if (valor <= 0)
            throw new RegraDeNegocioException("O valor da multa deve ser maior que zero.");

        var multa = new Multa { LocacaoId = Id, Tipo = tipo, Descricao = descricao, Valor = valor };
        multa.Aplicar();
        Multas.Add(multa);

        if (Status == StatusLocacao.FINALIZADA)
            ValorTotal = CalcularValorFinal();
        return multa;
    }
}
