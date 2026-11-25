namespace LocadoraApi.Endpoints;

public static class VeiculoEndpoints
{
    public static void MapVeiculos(this WebApplication app)
    {
        var g = app.MapGroup("/veiculos").WithTags("Veículos");

        g.MapGet("/", (Repositorio<Veiculo> repo, StatusVeiculo? status, CategoriaVeiculo? categoria) =>
        {
            var lista = repo.Listar().AsEnumerable();
            if (status.HasValue) lista = lista.Where(v => v.Status == status);
            if (categoria.HasValue) lista = lista.Where(v => v.Categoria == categoria);
            return Results.Ok(lista);
        });

        g.MapGet("/disponiveis", (Repositorio<Veiculo> repo) =>
            Results.Ok(repo.Listar().Where(v => v.EstaDisponivel())));

        g.MapGet("/{id:long}", (long id, Repositorio<Veiculo> repo) =>
            repo.Buscar(id) is { } v ? Results.Ok(v) : Results.NotFound(new { erro = "Veículo não encontrado." }));

        g.MapPost("/", (VeiculoRequest req, Repositorio<Veiculo> repo) =>
        {
            var erro = Validar(req);
            if (erro != null) return Results.BadRequest(new { erro });
            if (repo.Listar().Any(v => v.Placa.Equals(req.Placa, StringComparison.OrdinalIgnoreCase)))
                return Results.Conflict(new { erro = "Já existe um veículo com essa placa." });

            var veiculo = repo.Adicionar(new Veiculo
            {
                Placa = req.Placa.ToUpper(), Marca = req.Marca, Modelo = req.Modelo,
                AnoFabricacao = req.AnoFabricacao, Quilometragem = req.Quilometragem,
                ValorDiaria = req.ValorDiaria, Categoria = req.Categoria
            });
            return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
        });

        g.MapPut("/{id:long}", (long id, VeiculoRequest req, Repositorio<Veiculo> repo) =>
        {
            var veiculo = repo.Buscar(id);
            if (veiculo is null) return Results.NotFound(new { erro = "Veículo não encontrado." });
            var erro = Validar(req);
            if (erro != null) return Results.BadRequest(new { erro });

            veiculo.Placa = req.Placa.ToUpper();
            veiculo.Marca = req.Marca;
            veiculo.Modelo = req.Modelo;
            veiculo.AnoFabricacao = req.AnoFabricacao;
            veiculo.Quilometragem = req.Quilometragem;
            veiculo.ValorDiaria = req.ValorDiaria;
            veiculo.Categoria = req.Categoria;
            return Results.Ok(veiculo);
        });

        g.MapDelete("/{id:long}", (long id, Repositorio<Veiculo> repo) =>
        {
            var veiculo = repo.Buscar(id);
            if (veiculo is null) return Results.NotFound(new { erro = "Veículo não encontrado." });
            if (veiculo.Locacoes.Count > 0)
                return Results.Conflict(new { erro = "Veículo possui histórico de locações e não pode ser excluído." });
            repo.Remover(veiculo);
            return Results.NoContent();
        });

        g.MapPost("/{id:long}/manutencao", (long id, Repositorio<Veiculo> repo) =>
        {
            var veiculo = repo.Buscar(id);
            if (veiculo is null) return Results.NotFound(new { erro = "Veículo não encontrado." });
            veiculo.EnviarParaManutencao();
            return Results.Ok(veiculo);
        });

        g.MapPost("/{id:long}/liberar", (long id, Repositorio<Veiculo> repo) =>
        {
            var veiculo = repo.Buscar(id);
            if (veiculo is null) return Results.NotFound(new { erro = "Veículo não encontrado." });
            if (veiculo.Status == StatusVeiculo.LOCADO)
                return Results.BadRequest(new { erro = "Veículo locado só é liberado ao finalizar a locação." });
            veiculo.LiberarParaLocacao();
            return Results.Ok(veiculo);
        });
    }

    private static string? Validar(VeiculoRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Placa)) return "A placa é obrigatória.";
        if (string.IsNullOrWhiteSpace(r.Marca) || string.IsNullOrWhiteSpace(r.Modelo)) return "Marca e modelo são obrigatórios.";
        if (r.AnoFabricacao < 1990 || r.AnoFabricacao > DateTime.Now.Year + 1) return "Ano de fabricação inválido.";
        if (r.Quilometragem < 0) return "A quilometragem não pode ser negativa.";
        if (r.ValorDiaria <= 0) return "O valor da diária deve ser maior que zero.";
        return null;
    }
}
