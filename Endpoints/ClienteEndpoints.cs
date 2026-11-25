namespace LocadoraApi.Endpoints;

public static class ClienteEndpoints
{
    public static void MapClientes(this WebApplication app)
    {
        var g = app.MapGroup("/clientes").WithTags("Clientes");

        g.MapGet("/", (Repositorio<Cliente> repo) => Results.Ok(repo.Listar()));

        g.MapGet("/{id:long}", (long id, Repositorio<Cliente> repo) =>
            repo.Buscar(id) is { } c ? Results.Ok(c) : Results.NotFound(new { erro = "Cliente não encontrado." }));

        g.MapPost("/", (ClienteRequest req, Repositorio<Cliente> repo) =>
        {
            var erro = Validar(req);
            if (erro != null) return Results.BadRequest(new { erro });
            if (repo.Listar().Any(c => c.Cpf == req.Cpf))
                return Results.Conflict(new { erro = "Já existe um cliente com esse CPF." });

            var cliente = repo.Adicionar(new Cliente
            {
                Nome = req.Nome, Cpf = req.Cpf, Cnh = req.Cnh, CnhValidade = req.CnhValidade,
                Telefone = req.Telefone, Email = req.Email
            });
            return Results.Created($"/clientes/{cliente.Id}", cliente);
        });

        g.MapPut("/{id:long}", (long id, ClienteRequest req, Repositorio<Cliente> repo) =>
        {
            var cliente = repo.Buscar(id);
            if (cliente is null) return Results.NotFound(new { erro = "Cliente não encontrado." });
            var erro = Validar(req);
            if (erro != null) return Results.BadRequest(new { erro });

            cliente.Nome = req.Nome;
            cliente.Cpf = req.Cpf;
            cliente.Cnh = req.Cnh;
            cliente.CnhValidade = req.CnhValidade;
            cliente.Telefone = req.Telefone;
            cliente.Email = req.Email;
            return Results.Ok(cliente);
        });

        g.MapDelete("/{id:long}", (long id, Repositorio<Cliente> repo) =>
        {
            var cliente = repo.Buscar(id);
            if (cliente is null) return Results.NotFound(new { erro = "Cliente não encontrado." });
            if (cliente.GetTotalLocacoes() > 0)
                return Results.Conflict(new { erro = "Cliente possui locações e não pode ser excluído." });
            repo.Remover(cliente);
            return Results.NoContent();
        });

        // Resume as regras de negócio do cliente (CNH válida + sem atrasos)
        g.MapGet("/{id:long}/pode-alugar", (long id, Repositorio<Cliente> repo) =>
        {
            var cliente = repo.Buscar(id);
            if (cliente is null) return Results.NotFound(new { erro = "Cliente não encontrado." });
            return Results.Ok(new
            {
                cnhValida = cliente.CnhEstaValida(),
                possuiLocacoesAtrasadas = cliente.PossuiLocacoesAtrasadas(),
                podeAlugar = cliente.PodeAlugar(),
                totalLocacoes = cliente.GetTotalLocacoes()
            });
        });

        g.MapGet("/{id:long}/locacoes", (long id, Repositorio<Cliente> repo) =>
            repo.Buscar(id) is { } c ? Results.Ok(c.Locacoes) : Results.NotFound(new { erro = "Cliente não encontrado." }));
    }

    private static string? Validar(ClienteRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Nome)) return "O nome é obrigatório.";
        if (string.IsNullOrWhiteSpace(r.Cpf)) return "O CPF é obrigatório.";
        if (string.IsNullOrWhiteSpace(r.Cnh)) return "A CNH é obrigatória.";
        if (!string.IsNullOrWhiteSpace(r.Email) && !r.Email.Contains('@')) return "E-mail inválido.";
        return null;
    }
}
