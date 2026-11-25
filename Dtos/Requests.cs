namespace LocadoraApi.Dtos;

public record VeiculoRequest(
    string Placa, string Marca, string Modelo, int AnoFabricacao,
    double Quilometragem, decimal ValorDiaria, CategoriaVeiculo Categoria);

public record ClienteRequest(
    string Nome, string Cpf, string Cnh, DateOnly CnhValidade, string Telefone, string Email);

public record FuncionarioRequest(
    string Nome, string Cpf, string Matricula, string Cargo, decimal Salario, DateOnly DataAdmissao);

public record LocacaoRequest(long ClienteId, long VeiculoId, long FuncionarioId, int Dias);

/// <param name="DataDevolucao">Opcional – serve para simular devolução em atraso nos testes.</param>
public record FinalizarRequest(double KmDevolucao, DateTime? DataDevolucao);

public record MultaRequest(TipoMulta Tipo, string Descricao, decimal Valor);
