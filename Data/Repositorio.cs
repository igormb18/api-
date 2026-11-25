namespace LocadoraApi.Data;

public interface IEntidade
{
    long Id { get; set; }
}

/// <summary>Repositório genérico em memória (simula um banco de dados).</summary>
public class Repositorio<T> where T : class, IEntidade
{
    private readonly List<T> _itens = new();
    private readonly object _trava = new();
    private long _proximoId = 1;

    public List<T> Listar()
    {
        lock (_trava) return _itens.ToList();
    }

    public T? Buscar(long id)
    {
        lock (_trava) return _itens.FirstOrDefault(i => i.Id == id);
    }

    public T Adicionar(T item)
    {
        lock (_trava)
        {
            item.Id = _proximoId++;
            _itens.Add(item);
            return item;
        }
    }

    public bool Remover(T item)
    {
        lock (_trava) return _itens.Remove(item);
    }
}
