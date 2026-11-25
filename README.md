# 🚗 API de Locadora de Veículos

API REST de uma locadora de veículos feita com **ASP.NET Core Minimal API (C# / .NET 8)**.
Projeto acadêmico de Ciências da Computação, baseado no modelo UML com 5 classes
(`Veiculo`, `Cliente`, `Funcionario`, `Locacao`, `Multa`) e 4 enumerações.

Os dados ficam **em memória** (sem banco de dados), então tudo é zerado ao reiniciar a API.
Ao iniciar, a API já carrega 2 veículos, 1 cliente e 1 funcionário para facilitar os testes.

## Como executar

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
git clone <url-do-repositorio>
cd LocadoraApi
dotnet run
```

Abra a URL exibida no terminal (ex.: `http://localhost:5000`). Você será redirecionado ao
**Swagger** (`/swagger`), onde dá para testar todos os endpoints. Também há o arquivo
`requests.http` com exemplos prontos (VS Code com a extensão REST Client, ou Visual Studio).

## Estrutura

```
LocadoraApi/
├── Models/       # Entidades do UML + regras de negócio (métodos) e enums
├── Data/         # Repositório genérico em memória e dados iniciais
├── Dtos/         # Objetos de entrada (requests)
├── Endpoints/    # Rotas agrupadas por entidade
└── Program.cs    # Configuração da aplicação
```

## Endpoints

### Veículos (`/veiculos`)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/veiculos?status=&categoria=` | Lista (filtros opcionais) |
| GET | `/veiculos/disponiveis` | Lista só os disponíveis |
| GET | `/veiculos/{id}` | Busca por id |
| POST | `/veiculos` | Cadastra |
| PUT | `/veiculos/{id}` | Atualiza |
| DELETE | `/veiculos/{id}` | Remove (se não tiver histórico de locações) |
| POST | `/veiculos/{id}/manutencao` | Envia para manutenção |
| POST | `/veiculos/{id}/liberar` | Libera após manutenção |

### Clientes (`/clientes`)
| Método | Rota | Descrição |
|---|---|---|
| GET / POST | `/clientes` | Lista / cadastra |
| GET / PUT / DELETE | `/clientes/{id}` | Busca / atualiza / remove |
| GET | `/clientes/{id}/pode-alugar` | CNH válida? Tem atraso? Pode alugar? |
| GET | `/clientes/{id}/locacoes` | Histórico de locações |

### Funcionários (`/funcionarios`)
| Método | Rota | Descrição |
|---|---|---|
| GET / POST | `/funcionarios` | Lista / cadastra |
| GET / PUT | `/funcionarios/{id}` | Busca / atualiza |
| POST | `/funcionarios/{id}/desativar` | Desliga o funcionário (sem excluir) |
| GET | `/funcionarios/{id}/comissao?mes=&ano=` | Comissão (2% do faturamento do mês) |

### Locações (`/locacoes`)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/locacoes?status=` | Lista (filtro opcional) |
| GET | `/locacoes/{id}` | Busca por id |
| POST | `/locacoes` | Registra locação (`clienteId`, `veiculoId`, `funcionarioId`, `dias`) |
| POST | `/locacoes/{id}/finalizar` | Devolve o veículo e calcula o valor final |
| POST | `/locacoes/{id}/cancelar` | Cancela e libera o veículo |
| GET / POST | `/locacoes/{id}/multas` | Lista / aplica multa manual (dano, trânsito) |

### Multas (`/multas`)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/multas?pendentes=true` | Lista multas |
| GET | `/multas/{id}` | Busca por id |
| POST | `/multas/{id}/pagar` | Registra pagamento |
| GET | `/multas/{id}/juros?taxaDiaria=0.001` | Calcula juros (simples) |

## Regras de negócio implementadas

- **Veículo** só pode ser alugado se estiver `DISPONIVEL`; fica `LOCADO` durante a locação e volta a `DISPONIVEL` ao finalizar/cancelar.
- **Cliente** só aluga com CNH válida e sem locações atrasadas.
- **Funcionário** inativo não registra locações.
- **Locação** guarda um *snapshot* da diária do veículo; valor total = diárias × valor da diária + multas.
- **Atraso**: multa automática de 10% da diária por dia de atraso (tipo `ATRASO`) ao finalizar.
- **Multa**: pode ser `ATRASO`, `DANO` ou `MULTA_TRANSITO`; juros simples sobre multas pendentes.
- Violações de regra retornam **HTTP 400** com `{ "erro": "mensagem" }`.

## Exemplo de fluxo

1. `POST /locacoes` → `{ "clienteId": 1, "veiculoId": 1, "funcionarioId": 1, "dias": 3 }`
2. `GET /veiculos/1` → status `LOCADO`
3. `POST /locacoes/1/finalizar` → `{ "kmDevolucao": 15400 }`
4. `GET /veiculos/1` → status `DISPONIVEL`, quilometragem atualizada

Para testar **atraso**, envie `dataDevolucao` no finalizar (ex.: alguns dias após a data prevista).

## Tecnologias

C# · .NET 8 · ASP.NET Core Minimal API · Swagger (Swashbuckle)
