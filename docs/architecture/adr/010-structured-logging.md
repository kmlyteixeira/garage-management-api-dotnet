# ADR-010: Logs estruturados JSON

- Status: Proposed
- Data: 2026-09-08

## Contexto
A API já usa Serilog e correlation ID, mas precisa de formato consistente e proteção contra dados sensíveis.

## Decisão
Emitir logs JSON para stdout, com timestamp, nível, serviço, ambiente, traceId, correlationId e requestId quando disponíveis. CPF completo, JWT, passwords, connection strings e secrets nunca serão registrados.

## Alternativas consideradas
Texto livre é mais simples, mas dificulta consulta, correlação e alertas.

## Consequências
A configuração local e os agentes New Relic precisarão preservar os campos estruturados.
