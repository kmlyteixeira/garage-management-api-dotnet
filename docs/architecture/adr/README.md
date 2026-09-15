# ADRs

Decisões arquiteturais permanentes da solução Garage Management.

| ADR | Decisão | Status |
|---|---|---|
| [001](001-cloud-aws.md) | AWS como cloud | Proposed |
| [002](002-kubernetes-eks.md) | EKS como Kubernetes | Proposed |
| [003](003-database-rds-postgresql.md) | RDS PostgreSQL | Proposed |
| [004](004-authentication-cpf-jwt.md) | CPF + JWT emitido pela Lambda | Proposed |
| [005](005-authentication-lambda.md) | Lambda dedicada à emissão | Proposed |
| [006](006-api-gateway.md) | API Gateway HTTP | Proposed |
| [007](007-communication.md) | Comunicação síncrona HTTPS/JSON | Proposed |
| [008](008-hpa.md) | HPA para a API | Proposed |
| [009](009-observability-new-relic.md) | New Relic + OpenTelemetry | Proposed |
| [010](010-structured-logging.md) | Logs estruturados JSON | Proposed |
| [011](011-cicd-github-actions.md) | GitHub Actions | Proposed |
| [012](012-four-repositories.md) | Quatro repositórios | Proposed |

As decisões continuam `Proposed` até serem validadas com os responsáveis pela conta AWS, segurança, custos e operação.
