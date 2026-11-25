namespace LocadoraApi.Data;

/// <summary>Popula a API com alguns dados para facilitar os testes.</summary>
public static class DadosIniciais
{
    public static void Popular(WebApplication app)
    {
        var veiculos = app.Services.GetRequiredService<Repositorio<Veiculo>>();
        var clientes = app.Services.GetRequiredService<Repositorio<Cliente>>();
        var funcionarios = app.Services.GetRequiredService<Repositorio<Funcionario>>();

        veiculos.Adicionar(new Veiculo { Placa = "ABC1D23", Marca = "Fiat", Modelo = "Mobi", AnoFabricacao = 2022, Quilometragem = 15000, ValorDiaria = 120m, Categoria = CategoriaVeiculo.ECONOMICO });
        veiculos.Adicionar(new Veiculo { Placa = "EFG4H56", Marca = "Jeep", Modelo = "Compass", AnoFabricacao = 2023, Quilometragem = 8000, ValorDiaria = 280m, Categoria = CategoriaVeiculo.SUV });

        clientes.Adicionar(new Cliente { Nome = "Maria Silva", Cpf = "111.222.333-44", Cnh = "12345678900", CnhValidade = new DateOnly(2030, 5, 20), Telefone = "(49) 99999-0000", Email = "maria@email.com" });

        funcionarios.Adicionar(new Funcionario { Nome = "João Souza", Cpf = "555.666.777-88", Matricula = "F001", Cargo = "Atendente", Salario = 2500m, DataAdmissao = new DateOnly(2024, 3, 1) });
    }
}
