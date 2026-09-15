# ADR-006: API Gateway HTTP

- Status: Proposed
- Data: 2026-09-08

## Contexto
O fluxo exige uma entrada única para autenticação Lambda e API Kubernetes.

## Decisão
Usar API Gateway HTTP API para roteamento, throttling, logs e integração com Lambda e Load Balancer da API. Rotas públicas ficam limitadas ao login, healthcheck quando necessário e status público; rotas de negócio exigem JWT.

## Alternativas consideradas
REST API oferece mais recursos, porém com custo e configuração maiores. Expor o Load Balancer diretamente perde a borda centralizada.

## Consequências
TLS, domínio, authorizer JWT, CORS, throttling e access logs serão configurados em `garage-management-infra`.
