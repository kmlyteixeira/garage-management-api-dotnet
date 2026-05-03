# Garage Management API 

## Sobre o projeto
Repositório reservado para o desenvolvimento de um MVP do back-end do sistema de uma oficina, com foco na gestão de ordens de serviço, clientes e peças.

---

## 🧭 Arquitetura

O projeto segue o padrão **monolítico em camadas**, aplicando **Domain Driven Design (DDD)**.

## 🛠️ Tecnologias Utilizadas

* .NET (versão 8.0)
* ABP Framework (versão 8.3.0)
* Entity Framework Core
* Banco de dados relacional (PostgreSQL)
* Docker
* Docker Compose
* Swagger (OpenAPI)
* xUnit (testes automatizados)


## 🗄️ Banco de Dados

**Banco escolhido:** 

[![Postgres](https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white)](#)

### Justificativa:

* [Open-source e amplamente utilizado](https://www.postgresql.org/about/)
* [Suporte a JSON (flexibilidade para evolução)](https://www.postgresql.org/docs/current/datatype-json.html)
* [Integração simples com Docker](https://hub.docker.com/_/postgres)
* [Compatível com EF Core](https://www.npgsql.org/efcore/?tabs=onconfiguring)

---

## 🔨 Execute o projeto

### 🔹 Pré-requisitos

* Docker
* Docker Compose

---

### ▶️ Passo a passo

1. Crie o arquivo de variáveis de ambiente
    * Copie o arquivo de exemplo:

        ```bash
        cp .env.example .env
        ```
        Edite o .env conforme necessário.

2. Suba o ambiente com Docker
    ```bash
    docker-compose up --build
    ```
    O docker-compose está configurado para:
    * Provisionar o banco de dados da aplicação
    * Aplicar as migrations garantindo o schema atualizado
    * Rodar data-seed com dados para testes
    * Subir a API pronta para uso

**Após subir os containers:**

* API disponível em:
  `http://localhost:8080` 

* Swagger:
  `http://localhost:8080/swagger`

---

## 📋 Documentação da API

A API está documentada utilizando **Swagger (OpenAPI)**.

1. Acesse:

    ```
    http://localhost:8080/swagger
    ```
2. Autentique: 
* Através do botão *Authorize* 
* Insira as credenciais presentes no `.env` para logar com o usuário **admin** *(ou crie um novo usuário)*
    ```
    IdentityClients__Default__UserName=
    IdentityClients__Default__UserPassword=
    ```

Através do swagger você pode:

* Visualizar endpoints
* Testar requisições
* Entender contratos de entrada/saída

---

## 🔬 Testes Automatizados

O projeto possui testes automatizados com foco nos **domínios críticos**.

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=kmlyteixeira_garage-management-api-dotnet&metric=coverage&token=ad876c9d0e30de7ccbbf7e98df620074ea2c6202)](https://sonarcloud.io/summary/new_code?id=kmlyteixeira_garage-management-api-dotnet)

### ▶️ Executar testes

```bash
dotnet test
```

---

## 🔍 Análise de Vulnerabilidades (OWASP ZAP)

O projeto possui uma configuração para realizar a análise de vulnerabilidades localmente.

**Arquivos relacionados:**

`docker-compose.scan.yml`

`security\run-security-scan.ps1`

`security\zap-config.conf`

### ▶️ Executar scanner

```bash
powershell -ExecutionPolicy Bypass -File security/run-security-scan.ps1
```

* O resultado da execução será gerado em dois formatos: HTML e JSON.
* Caminho para visualizar o resultado do scan: `garage-management-api-dotnet\reports\security`

---

## 📁 Estrutura do Projeto

```
docs/                                           <!-- Documentação DDD -->
└── architecture/
    ├── domain-storytelling
    ├── event-storming
    └── glossary
security/                                       <!-- Scripts para execução do OWASP ZAP via docker -->
reports/                                        <!-- Resultado da execução da análise de vulnerabilidades -->
└── security/
src/
├── GarageManagement.Application/              <!-- Orquestra os casos de uso da aplicação, coordenando regras de negócio -->
├── GarageManagement.Application.Contracts/    <!-- Define interfaces e DTOs que expõem os contratos da aplicação -->
├── GarageManagement.Domain/                   <!-- Contém as entidades e regras de negócio centrais do sistema -->
│   └── Entities
├── GarageManagement.Domain.Shared/            <!-- Armazena enums, constantes e definições compartilhadas -->
├── GarageManagement.EntityFrameworkCore/      <!-- Implementa a persistência e integração com o banco de dados -->
│   └── EntityFrameworkCore
│       ├── Configurations                    <!-- Define o mapeamento entre entidades e tabelas -->
│       └── Migrations                        <!-- Controla a evolução do schema do banco de dados -->
├── GarageManagement.HttpApi/                  <!-- Expõe os endpoints REST da aplicação via controllers -->
├── GarageManagement.HttpApi.Client/
├── GarageManagement.HttpApi.Host/             <!-- Configura e inicializa a aplicação (middlewares, DI, etc.) -->
└── GarageManagement.DbMigrator/               <!-- Executa migrations e garante a atualização do banco de dados -->

test/                                           <!-- Testes unitários e Testes de Integração -->
```
---

## 📃 Documentações

1️⃣ [Event Storming](https://github.com/kmlyteixeira/garage-management-api-dotnet/tree/master/docs/architecture/event-storming)

2️⃣ [Domain StoryTelling](https://github.com/kmlyteixeira/garage-management-api-dotnet/tree/master/docs/architecture/domain-storytelling)

3️⃣ [Dicionário Linguagem Ubíqua](https://github.com/kmlyteixeira/garage-management-api-dotnet/tree/master/docs/architecture/glossary)

4️⃣ [Relatório de Análise de Vulnerabilidades](https://github.com/kmlyteixeira/garage-management-api-dotnet/tree/master/reports/security)

## 📑 Referências

1️⃣ [ABP Framework](https://abp.io/get-started)

2️⃣ Documento de Especificação Tech Challenge 1ª Fase FIAP

3️⃣ Módulos 1ª Fase SOAT FIAP
