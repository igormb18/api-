namespace LocadoraApi.Models;

public class Multa
{
    // Contador simples para gerar ids (suficiente para um projeto sem banco de dados).
    private static long _sequencia;

    public long Id { get; } = Interlocked.Increment(ref _sequencia);
    public long LocacaoId { get; set; }
    public TipoMulta Tipo { get; set; }
    public string Descricao { get; set; } = "";
    public decimal Valor { get; set; }
    public DateOnly DataAplicacao { get; set; }
    public bool Paga { get; set; }

    public void Aplicar()
    {
        DataAplicacao = DateOnly.FromDateTime(DateTime.Today);
        Paga = false;
    }

    public void RegistrarPagamento()
    {
        if (Paga)
            throw new RegraDeNegocioException("Esta multa já foi paga.");
        Paga = true;
    }

    public bool EstaPendente() => !Paga;

    /// <summary>Juros simples: valor × taxa diária × dias desde a aplicação.</summary>
    public decimal CalcularJuros(decimal taxaDiaria)
    {
        if (!EstaPendente()) return 0;
        var dias = DateOnly.FromDateTime(DateTime.Today).DayNumber - DataAplicacao.DayNumber;
        return Math.Round(Valor * taxaDiaria * Math.Max(0, dias), 2);
    }
}
