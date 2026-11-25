using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Enums aparecem como texto no JSON (ex.: "DISPONIVEL") em vez de números
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// "Banco de dados" em memória: um repositório por entidade
builder.Services.AddSingleton<Repositorio<Veiculo>>();
builder.Services.AddSingleton<Repositorio<Cliente>>();
builder.Services.AddSingleton<Repositorio<Funcionario>>();
builder.Services.AddSingleton<Repositorio<Locacao>>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Regras de negócio violadas viram HTTP 400 com mensagem clara
app.Use(async (contexto, proximo) =>
{
    try
    {
        await proximo();
    }
    catch (RegraDeNegocioException ex)
    {
        contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
        await contexto.Response.WriteAsJsonAsync(new { erro = ex.Message });
    }
});

DadosIniciais.Popular(app);

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapVeiculos();
app.MapClientes();
app.MapFuncionarios();
app.MapLocacoes();
app.MapMultas();

app.Run();
