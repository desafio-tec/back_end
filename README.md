## 🛡️ LH Tecnologia - Auth Backend API

API RESTful robusta desenvolvida para o Desafio Técnico da LH Tecnologia. Este projeto provê serviços de autenticação segura, gestão de usuários e persistência de dados, servindo como a espinha dorsal para o Frontend em React.

## 📋 Sobre o Projeto

Este Backend foi arquitetado para resolver o problema de **Autenticação e Autorização** de forma escalável e segura. Ele não apenas salva dados, mas implementa regras de negócio, criptografia e proteção contra abusos.

### Principais Funcionalidades

-   **Autenticação JWT (JSON Web Token):** Sistema de login stateless, ideal para arquiteturas modernas e microserviços.
    
-   **Segurança de Senhas:** Utilização de **BCrypt** para hash de senhas (nenhuma senha é salva em texto puro).
    
-   **Arquitetura em Camadas:**
    
    -   **Controllers:** Gerenciam as requisições HTTP.
        
    -   **Services:** Encapsulam a lógica de negócio (ex: geração de tokens).
        
    -   **Repositories:** Abstraem o acesso ao banco de dados.
        
    -   **DTOs (Data Transfer Objects):** Filtram os dados que entram e saem da API, protegendo informações sensíveis.
        
-   **Resiliência e Segurança:**
    
    -   **Rate Limiting:** Proteção contra ataques de força bruta e DDoS (limite de requisições por IP).
        
    -   **CORS:** Configurado para aceitar apenas origens confiáveis (Frontends específicos).
        
-   **Documentação Automática:** Swagger UI integrado para testes e exploração dos endpoints.
    

## 🚀 Tecnologias Utilizadas

-   **Linguagem:** C# (.NET 9)
    
-   **Framework:** ASP.NET Core Web API
    
-   **Banco de Dados:** PostgreSQL (Hospedado na Neon Tech)
    
-   **ORM:** Entity Framework Core (Code First)
    
-   **Containerização:** Docker
    
-   **Hospedagem:** https://www.google.com/search?q=Render.com
    

## 🛠️ Arquitetura da Solução

O projeto segue o padrão de separação de responsabilidades:

```
AuthApi/
├── Controllers/    # Endpoints da API (Entrada)
├── Services/       # Regras de Negócio (ex: TokenService)
├── Repositories/   # Acesso ao Banco de Dados (UserRepository)
├── Models/         # Entidades do Banco (Tabelas)
├── DTOs/           # Objetos de Transferência (Input/Output)
└── Data/           # Contexto do Entity Framework



```

## ☁️ Guia de Deploy (https://www.google.com/search?q=Render.com)

Esta aplicação foi otimizada para CI/CD (Integração e Entrega Contínuas) usando o **Render**.

### Pré-requisitos de Infraestrutura

1.  **Banco de Dados:** Uma instância PostgreSQL criada (recomendado: **Neon.tech**).
    
2.  **Repositório:** Código fonte hospedado no GitHub.
    

### Passo a Passo para Deploy

1.  **Crie um Web Service no Render:**
    
    -   Conecte sua conta do GitHub.
        
    -   Selecione o repositório `back_end`.
        
2.  **Configurações de Build:**
    
    -   **Runtime:** Docker
        
    -   **Region:** Escolha a mais próxima (ex: Ohio ou Frankfurt).
        
    -   **Branch:** `main`
        
    -   **Root Directory:** `AuthApi` (Muito importante: pois o Dockerfile está dentro desta pasta).
        
3.  Variáveis de Ambiente (Environment Variables):
    
    Adicione as seguintes chaves na aba "Environment":
    
    **Chave**
    `ConnectionStrings__DefaultConnection`
    **Valor (Exemplo/Descrição)**
    `Host=ep-xyz.aws.neon.tech;Database=neondb;Username=...;SSL Mode=Require;Trust Server Certificate=true`
    
    **Chave**
    `Jwt__Key`
     **Valor (Exemplo/Descrição)**
    Uma frase longa e aleatória para assinar os tokens (ex: `minha_chave_super_secreta_e_segura_123`)
    
    **Chave**
    `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE`
    **Valor (Exemplo/Descrição)**
    `false` (Essencial para evitar erros de I/O no Linux do Render)
    
    **Chave**
    `PORT`
    **Valor (Exemplo/Descrição)**
    `8080` (Opcional, o Render costuma detectar)
    
4.  **Finalizar:**
    
    -   Clique em **Create Web Service**.
        
    -   O Render irá baixar o repositório, construir a imagem Docker e iniciar a aplicação.
        
    -   O banco de dados será atualizado automaticamente (Migrations) na inicialização.
        

## 🧪 Como Testar (Swagger)

Após o deploy, a documentação interativa estará disponível em:

```
https://back-end-443z.onrender.com/swagger/index.html



```

1.  Use o endpoint `POST /api/Auth/register` para criar um usuário.
    
2.  Use o endpoint `POST /api/Auth/login` para receber um **Token JWT**.
    
3.  Para testar rotas protegidas, clique no cadeado **Authorize** no topo do Swagger e insira: `Bearer <seu_token>`.
    

## 💻 Execução Local (Desenvolvimento)

Caso queira rodar a aplicação em sua máquina para testes:

1.  Certifique-se de ter o **.NET SDK 9.0** instalado.
    
2.  Clone o repositório.
    
3.  Navegue até a pasta: `cd AuthApi`.
    
4.  Rode o comando:
    
    ```
    dotnet run
    
    
    
    ```
    
5.  Acesse `http://localhost:5248/swagger` no navegador.
    

**Desenvolvido por Lucas Henrique**
