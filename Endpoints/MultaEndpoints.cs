namespace LocadoraApi.Endpoints;

public static class MultaEndpoints
{
    public static void MapMultas(this WebApplication app)
    {
        var g = app.MapGroup("/multas").WithTags("Multas");

        // As multas ficam dentro das locações; aqui buscamos em todas
        g.MapGet("/", (Repositorio<Locacao> repo, bool? pendentes) =>
        {
            var multas = repo.Listar().SelectMany(l => l.Multas);
            if (pendentes == true) multas = multas.Where(m => m.EstaPendente());
            return Results.Ok(multas);
        });

        g.MapGet("/{id:long}", (long id, Repositorio<Locacao> repo) =>
            Buscar(repo, id) is { } m ? Results.Ok(m) : Results.NotFound(new { erro = "Multa não encontrada." }));

        g.MapPost("/{id:long}/pagar", (long id, Repositorio<Locacao> repo) =>
        {
            var multa = Buscar(repo, id);
            if (multa is null) return Results.NotFound(new { erro = "Multa não encontrada." });
            multa.RegistrarPagamento();
            return Results.Ok(multa);
        });

        g.MapGet("/{id:long}/juros", (long id, decimal taxaDiaria, Repositorio<Locacao> repo) =>
        {
            var multa = Buscar(repo, id);
            if (multa is null) return Results.NotFound(new { erro = "Multa não encontrada." });
            if (taxaDiaria < 0) return Results.BadRequest(new { erro = "A taxa diária não pode ser negativa." });
            return Results.Ok(new { multaId = multa.Id, valor = multa.Valor, juros = multa.CalcularJuros(taxaDiaria) });
        });
    }

    private static Multa? Buscar(Repositorio<Locacao> repo, long id) =>
        repo.Listar().SelectMany(l => l.Multas).FirstOrDefault(m => m.Id == id);
}
