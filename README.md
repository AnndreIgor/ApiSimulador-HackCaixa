# 🧪 ApiSimulador

**ApiSimulador** é uma aplicação ASP.NET Core desenvolvida para simular operações de cálculo e retorno de dados via API REST. Ela foi projetada para rodar em ambiente containerizado com suporte a HTTPS, ideal para testes, integração com sistemas externos e validação de lógica de negócio.

---

## 📦 Tecnologias Utilizadas

| Componente        | Versão        |
|-------------------|---------------|
| .NET SDK          | 8.0           |
| ASP.NET Core      | 8.0           |
| Docker            | 24.x ou superior |
| Docker Compose    | 2.x ou superior |
| MySQL             | 8.0           |

---

## 🚀 Como Executar a Aplicação com Docker Compose

1. Extraia a pasta enviada como resposta da etapa.

2. Construa os containers
```bash
docker-compose up --build
```

3. Acesse via navegador a documentação da API: https://localhost/swagger/index.html



