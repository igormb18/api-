namespace LocadoraApi.Endpoints;

public static class FuncionarioEndpoints
{
    public static void MapFuncionarios(this WebApplication app)
    {
        var g = app.MapGroup("/funcionarios").WithTags("Funcionários");

        g.MapGet("/", (Repositorio<Funcionario> repo) => Results.Ok(repo.Listar()));

        g.MapGet("/{id:long}", (long id, Repositorio<Funcionario> repo) =>
            repo.Buscar(id) is { } f ? Results.Ok(f) : Results.NotFound(new { erro = "Funcionário não encontrado." }));

        g.MapPost("/", (FuncionarioRequest req, Repositorio<Funcionario> repo) =>
        {
            var erro = Validar(req);
            if (erro != null) return Results.BadRequest(new { erro });
            if (repo.Listar().Any(f => f.Matricula == req.Matricula || f.Cpf == req.Cpf))
                return Results.Conflict(new { erro = "Já existe um funcionário com essa matrícula ou CPF." });

            var funcionario = repo.Adicionar(new Funcionario
            {
                Nome = req.Nome, Cpf = req.Cpf, Matricula = req.Matricula, Cargo = req.Cargo,
                Salario = req.Salario, DataAdmissao = req.DataAdmissao
            });
            return Results.Created($"/funcionarios/{funcionario.Id}", funcionario);
        });

        g.MapPut("/{id:long}", (long id, FuncionarioRequest req, Repositorio<Funcionario> repo) =>
        {
            var funcionario = repo.Buscar(id);
            if (funcionario is null) return Results.NotFound(new { erro = "Funcionário não encontrado." });
            var erro = Validar(req);
            if (erro != null) return Results.BadRequest(new { erro });

            funcionario.Nome = req.Nome;
            funcionario.Cpf = req.Cpf;
            funcionario.Matricula = req.Matricula;
            funcionario.Cargo = req.Cargo;
            funcionario.Salario = req.Salario;
            funcionario.DataAdmissao = req.DataAdmissao;
            return Results.Ok(funcionario);
        });

        // Desligamento = desativar (não exclui, preserva o histórico)
        g.MapPost("/{id:long}/desativar", (long id, Repositorio<Funcionario> repo) =>
        {
            var funcionario = repo.Buscar(id);
            if (funcionario is null) return Results.NotFound(new { erro = "Funcionário não encontrado." });
            funcionario.Ativo = false;
            return Results.Ok(funcionario);
        });

        g.MapGet("/{id:long}/comissao", (long id, int mes, int ano, Repositorio<Funcionario> repo) =>
        {
            var funcionario = repo.Buscar(id);
            if (funcionario is null) return Results.NotFound(new { erro = "Funcionário não encontrado." });
            if (mes < 1 || mes > 12) return Results.BadRequest(new { erro = "Mês inválido." });

            return Results.Ok(new
            {
                mes,
                ano,
                totalLocacoes = funcionario.GetTotalLocacoesRealizadas(mes, ano),
                comissao = funcionario.CalcularComissao(mes, ano)
            });
        });
    }

    private static string? Validar(FuncionarioRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Nome)) return "O nome é obrigatório.";
        if (string.IsNullOrWhiteSpace(r.Cpf)) return "O CPF é obrigatório.";
        if (string.IsNullOrWhiteSpace(r.Matricula)) return "A matrícula é obrigatória.";
        if (r.Salario < 0) return "O salário não pode ser negativo.";
        return null;
    }
}
