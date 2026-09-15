# ADR-007: Comunicação síncrona HTTPS/JSON

- Status: Proposed
- Data: 2026-09-08

## Contexto
Os casos atuais são APIs HTTP convencionais e não há broker ou processamento assíncrono implementado.

## Decisão
Manter comunicação síncrona HTTPS/JSON entre cliente, API Gateway, Lambda e API. Correlation ID será propagado por header e logs.

## Alternativas consideradas
Mensageria e eventos podem ser adicionados para notificações futuras, mas introduziriam infraestrutura não existente.

## Consequências
A latência e os timeouts entre Gateway, Lambda, API e banco precisam ser monitorados. Nenhum componente assíncrono será inventado nesta fase.
