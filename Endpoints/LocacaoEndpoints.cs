namespace LocadoraApi.Endpoints;

public static class LocacaoEndpoints
{
    public static void MapLocacoes(this WebApplication app)
    {
        var g = app.MapGroup("/locacoes").WithTags("Locações");

        g.MapGet("/", (Repositorio<Locacao> repo, StatusLocacao? status) =>
        {
            var lista = repo.Listar().AsEnumerable();
            if (status.HasValue) lista = lista.Where(l => l.Status == status);
            return Results.Ok(lista);
        });

        g.MapGet("/{id:long}", (long id, Repositorio<Locacao> repo) =>
            repo.Buscar(id) is { } l ? Results.Ok(l) : Results.NotFound(new { erro = "Locação não encontrada." }));

        // Cria a locação: o funcionário valida as regras (CNH, atrasos, disponibilidade)
        g.MapPost("/", (LocacaoRequest req, Repositorio<Locacao> locacoes, Repositorio<Cliente> clientes,
                        Repositorio<Veiculo> veiculos, Repositorio<Funcionario> funcionarios) =>
        {
            var cliente = clientes.Buscar(req.ClienteId);
            if (cliente is null) return Results.NotFound(new { erro = "Cliente não encontrado." });
            var veiculo = veiculos.Buscar(req.VeiculoId);
            if (veiculo is null) return Results.NotFound(new { erro = "Veículo não encontrado." });
            var funcionario = funcionarios.Buscar(req.FuncionarioId);
            if (funcionario is null) return Results.NotFound(new { erro = "Funcionário não encontrado." });

            var locacao = funcionario.RegistrarLocacao(cliente, veiculo, req.Dias);
            locacoes.Adicionar(locacao);
            return Results.Created($"/locacoes/{locacao.Id}", locacao);
        });

        g.MapPost("/{id:long}/finalizar", (long id, FinalizarRequest req, Repositorio<Locacao> repo) =>
        {
            var locacao = repo.Buscar(id);
            if (locacao is null) return Results.NotFound(new { erro = "Locação não encontrada." });
            locacao.Finalizar(req.KmDevolucao, req.DataDevolucao);
            return Results.Ok(locacao);
        });

        g.MapPost("/{id:long}/cancelar", (long id, Repositorio<Locacao> repo) =>
        {
            var locacao = repo.Buscar(id);
            if (locacao is null) return Results.NotFound(new { erro = "Locação não encontrada." });
            locacao.Cancelar();
            return Results.Ok(locacao);
        });

        // Multas da locação
        g.MapGet("/{id:long}/multas", (long id, Repositorio<Locacao> repo) =>
            repo.Buscar(id) is { } l ? Results.Ok(l.Multas) : Results.NotFound(new { erro = "Locação não encontrada." }));

        g.MapPost("/{id:long}/multas", (long id, MultaRequest req, Repositorio<Locacao> repo) =>
        {
            var locacao = repo.Buscar(id);
            if (locacao is null) return Results.NotFound(new { erro = "Locação não encontrada." });
            if (string.IsNullOrWhiteSpace(req.Descricao))
                return Results.BadRequest(new { erro = "A descrição da multa é obrigatória." });

            var multa = locacao.AdicionarMulta(req.Tipo, req.Descricao, req.Valor);
            return Results.Created($"/multas/{multa.Id}", multa);
        });
    }
}
