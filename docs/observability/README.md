# Observabilidade

A API emite logs JSON para stdout e possui instrumentação OpenTelemetry opcional via `OTEL_EXPORTER_OTLP_ENDPOINT`. A Lambda emite logs estruturados sem CPF completo ou JWT.

## Dashboards e alertas

Os templates versionados em `new-relic/` são modelos de definição e não representam dashboards implantados. A implantação exige conta, licença, API key e nomes reais de entidades.

Métricas de negócio por status e tempo de ciclo de OS ainda exigem instrumentação explícita no domínio; o template usa tráfego real dos endpoints como métrica disponível hoje.

Thresholds iniciais propostos:

- HTTP 5xx: alerta acima de 5% por 5 minutos.
- Latência P95: alerta acima de 1 segundo por 10 minutos.
- Uptime: alerta abaixo de 99%.
- Pod restarts: alerta acima de 3 em 15 minutos.
- CPU: alerta acima de 80% por 10 minutos.
- Memória: alerta acima de 85% por 10 minutos.
- Banco: alerta em falha de conexão ou disponibilidade abaixo de 99%.
