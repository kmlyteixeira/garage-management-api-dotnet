# ADR-012: Quatro repositórios

- Status: Proposed
- Data: 2026-09-08

## Contexto
Aplicação, autenticação serverless, infraestrutura Kubernetes/cloud e banco possuem ciclos e permissões diferentes.

## Decisão
Separar em `garage-management-api-dotnet`, `garage-management-lambda`, `garage-management-infra` e `garage-management-database`. A API continua sendo o ponto central da documentação.

## Alternativas consideradas
Monorepo preserva transações atômicas, mas mantém responsabilidades e acessos excessivamente acoplados. Cópias de código entre repositórios são proibidas.

## Consequências
Haverá versionamento e pipelines independentes. Contratos de integração precisarão ser documentados e testados entre repositórios.
