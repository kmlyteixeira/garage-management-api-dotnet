# RFC-004: Estratégia de observabilidade

- Status: Proposed

## Problema
É necessário observar API, Lambda, Kubernetes, banco, integrações, logs, traces, dashboards e alertas.

## Contexto
A solução tem Serilog e correlation ID, mas ainda não possui integração real com New Relic ou Datadog.

## Alternativas
New Relic, Datadog ou apenas CloudWatch/OpenTelemetry sem SaaS.

## Critérios
Integração AWS/EKS/Lambda, custo, logs estruturados, métricas, traces, dashboards e alertas.

## Solução proposta
New Relic, com OpenTelemetry na aplicação e integrações AWS/Kubernetes.

## Impactos e riscos
Depende de licença, chaves e política de retenção. Dados pessoais não podem ser enviados aos logs.

## Plano
Definir conta e retenção, instrumentar API/Lambda, criar dashboards operacionais e alertas com thresholds documentados.
