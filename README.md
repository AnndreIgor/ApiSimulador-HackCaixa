# 🧪 ApiSimulador — .NET 8 (MySQL + EF Core)

Aplicação **ASP.NET Core 8** para simulação de empréstimos com cálculo **SAC** e **PRICE**, armazenamento de simulações, validação padronizada de erros, e integrações opcionais de **telemetria** (log de requisições no banco) e **Azure Event Hubs**.

Aplicação desenvolvida como avaliação para o Hackathon CAIXA 2025.


---

## 📦 Stack

| Componente | Uso |
|---|---|
| **.NET SDK 8.0** | API Web (ASP.NET Core) |
| **EF Core 8** | Acesso a dados |
| **MySQL 8** | Simulações, Parcelas e RequestLogs |
| **SQL Server** | Produtos de cédito de acordo com os parâmtros|
| **Swagger (Swashbuckle)** | Documentação interativa |
| **xUnit + WebApplicationFactory** | Testes de integração |
| **Azure Event Hubs** | Publicação das simulações realizadas |

---

## 🗂️ Estrutura (resumo)

```
ApiSimulador.sln
├─ ApiSimulador/                        # Projeto Web API
│  ├─ Controllers/SimuladorController.cs
│  ├─ Context/
│  │  ├─ MySqlDbContext.cs             # SIMULACAO, PARCELA, RequestLog
│  │  └─ SqlServerDbContext.cs         # PRODUTO 
│  ├─ Models/                          # Simulacao, Parcela, Produto, RequestLog
│  ├─ Contracts/
│  │  ├─ Requests/                     # SimulacaoRequest, ListaSimulacaoQuery, etc.
│  │  └─ Responses/                    # ResumoSimulacoesResponse, etc.
│  ├─ Middlewares/                     # Log de requisições, EventHub capture
│  ├─ Migrations/                      # MySqlDb & SqlServerDb
│  ├─ wwwroot/swagger/                 # Customização de UI do Swagger
│  ├─ appsettings*.json
│  └─ Program.cs
└─ ApiSimulador.Tests.Unit/            # Testes de unidade
```

---

## ⚙️ Configuração

> O projeto suporta configuração via `appsettings.json`.

### Certificado de desenvolvimento (HTTPS)

> Foi utilizado certificado autoassinado para suporte HTTPS

---

## 🗄️ Banco de Dados & Migrações

O projeto possui **dois DbContexts**:
- **MySqlDbContext** → tabelas `SIMULACAO`, `PARCELA`, `RequestLogs` (Logs colocados na mesma base para simplificação).
- **SqlServerDbContext** → tabela `PRODUTO`.


## 🐳 Executando com Docker

### Dockerfile
Já incluso na raiz. Publica e executa a API em runtime .NET 8.

> Requisitos:
- Ter o docker instalado (https://www.docker.com/products/docker-desktop/)
- Acesso a internet para download das imagens necessárias.
### Compose (exemplo)

Subir tudo:

```bash
docker compose up --build
```

> Acessar o swagger da API: https://localhost/swagger/index.html (Deixado sem autenticação para facilitar a avaliação)

Como o certificado é autoassinado o navegador emite um aviso de não-seguro, bastar aceitar o risco e continuar. O passo a passo exato varia de acordo com o navegador escolhido.

---

## 🔀 Versionamento da API

A API usa segmentação de versão no caminho (Asp.Versioning):
```
/api/v1/...
```

---

## 📚 Endpoints principais (resumo)

### 1) **POST** `/api/v1/simulador/simular`  
Realiza uma simulação a partir do **valor** (decimal) e **prazo** (meses).  
**Body (JSON)** — `SimulacaoRequest`:
```json
{
  "valor": 900.0,
  "prazo": 12
}
```

**Resposta (exemplo simplificado)**:
```json
{
  "codigoProduto": 1,
  "descricaoProduto": "Produto Teste",
  "taxaJuros": 0.02,
  "linhas": [
    { "tipo": "SAC", "parcelas": [ /* ... */ ] },
    { "tipo": "PRICE", "parcelas": [ /* ... */ ] }
  ]
}
```

> Validações server-side garantem: `valor > 0`, `prazo > 0`. Mensagens são padronizadas.

---

### 2) **GET** `/api/v1/simulador/simulacoes?limit=50&offset=0`  
Lista simulações paginadas. Retorna **HTTP 206 Partial Content** com links **HATEOAS**.

**Resposta (exemplo)**:
```json
{
  "pagina": 1,
  "qtdRegistros": 120,
  "qtdRegistrosPagina": 50,
  "registros": [ /* itens */ ],
  "links": [
    { "rel": "first", "href": "http://localhost/api/v1/simulador/simulacoes?limit=50&offset=0" },
    { "rel": "next",  "href": "http://localhost/api/v1/simulador/simulacoes?limit=50&offset=50" },
    { "rel": "last",  "href": "http://localhost/api/v1/simulador/simulacoes?limit=50&offset=100" }
  ]
}
```

Parâmetros de consulta:  
- `limit` (1–200)  
- `offset` (≥ 0)  

---

### 3) **GET** `/health` *(ou `/`)*  
Exposição do **Health Check** 

---

## ❗ Padrão de erros e validação

A API centraliza erros de **ModelState** e exceções em formato consistente.

**Exemplo (HTTP 400)**:
```json
{
  "erro": 400,
  "errors": [
    { "message": "O valor do emprestimo deve ser maior que zero" }
  ]
}
```

---

## 🛰️ Telemetria e Logs de Requisição

- **RequestLoggingMiddleware**: grava cada requisição em `RequestLogs` (MySql).
- **EventHubResponseCaptureMiddleware**: publica a **resposta** de rotas-alvo (por ex. `/api/v1/simulador/simular`) no **Azure Event Hubs**

---

## ✅ Testes

### Testes de Unidade
- Checam os cálculos internos (**SAC** e **PRICE**) do `SimuladorController`.

---

## Easter EGG
Acesse: https://localhost/api/v1/simulador/easter-egg
É possível filtrar por personagens na query string (?persogagem=nome-do-personagem)

## 📄 Licença

MIT License
