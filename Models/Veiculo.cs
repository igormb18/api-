using System.Text.Json.Serialization;

namespace LocadoraApi.Models;

public class Veiculo : IEntidade
{
    public long Id { get; set; }
    public string Placa { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public int AnoFabricacao { get; set; }
    public double Quilometragem { get; set; }
    public decimal ValorDiaria { get; set; }
    public CategoriaVeiculo Categoria { get; set; }
    public StatusVeiculo Status { get; set; } = StatusVeiculo.DISPONIVEL;

    [JsonIgnore]
    public List<Locacao> Locacoes { get; } = new();

    public bool EstaDisponivel() => Status == StatusVeiculo.DISPONIVEL;

    public void RegistrarQuilometragem(double km)
    {
        if (km < Quilometragem)
            throw new RegraDeNegocioException("A nova quilometragem não pode ser menor que a atual.");
        Quilometragem = km;
    }

    public decimal CalcularValorLocacao(int dias)
    {
        if (dias <= 0)
            throw new RegraDeNegocioException("A quantidade de dias deve ser maior que zero.");
        return ValorDiaria * dias;
    }

    public void EnviarParaManutencao()
    {
        if (Status == StatusVeiculo.LOCADO)
            throw new RegraDeNegocioException("Não é possível enviar para manutenção um veículo locado.");
        Status = StatusVeiculo.EM_MANUTENCAO;
    }

    public void LiberarParaLocacao() => Status = StatusVeiculo.DISPONIVEL;

    public void MarcarComoLocado() => Status = StatusVeiculo.LOCADO;
}
