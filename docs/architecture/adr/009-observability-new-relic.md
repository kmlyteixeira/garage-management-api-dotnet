# ADR-009: New Relic e OpenTelemetry

- Status: Proposed
- Data: 2026-09-08

## Contexto
A solução precisa de logs, métricas, traces, dashboards e alertas para API, Lambda, Kubernetes e banco. A documentação atual já cita New Relic, mas não há integração real.

## Decisão
Adotar New Relic como ferramenta de observabilidade, usando OpenTelemetry onde suportado e integração nativa AWS/Kubernetes para infraestrutura.

## Alternativas consideradas
Datadog oferece forte integração Kubernetes, mas a documentação atual já direciona New Relic. CloudWatch isolado não cobre bem a visão distribuída exigida.

## Consequências
A decisão depende de conta, licença, região e custo. Até a integração ser aplicada, dashboards e alertas serão apenas pendências documentais.
