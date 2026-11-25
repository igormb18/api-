namespace LocadoraApi.Models;

/// <summary>Lançada quando uma regra de negócio é violada (vira HTTP 400 na API).</summary>
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string mensagem) : base(mensagem) { }
}
