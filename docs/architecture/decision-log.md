# Registro da Etapa 2

## Decisões propostas

- Cloud: AWS.
- Compute da API: EKS.
- Banco: RDS PostgreSQL.
- Entrada: API Gateway HTTP.
- Emissão de token: Lambda dedicada.
- Validação: authorizer do Gateway e validação defensiva na API.
- Observabilidade: New Relic com OpenTelemetry.
- CI/CD: GitHub Actions com ambientes protegidos.
- Migrations: permanecem na API e são executadas pelo DbMigrator.
- Separação: quatro repositórios independentes.

## O que ainda não é fato

Nenhum API Gateway, Lambda de produção, dashboard New Relic, alerta, novo repositório Git ou ambiente cloud foi criado por esta etapa. Os documentos registram a arquitetura aprovada para orientar a implementação incremental.
