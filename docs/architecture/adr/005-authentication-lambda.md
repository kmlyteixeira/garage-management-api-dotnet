# ADR-005: Lambda dedicada à autenticação

- Status: Proposed
- Data: 2026-09-08

## Contexto
A função de autenticação é independente do ciclo de negócio da API e precisa escalar conforme demanda de login.

## Decisão
Implementar a Lambda em `garage-management-lambda`, com validação de CPF, consulta somente leitura ao cliente, verificação de status e emissão de JWT.

## Alternativas consideradas
Colocar o fluxo na API ou manter OpenIddict. Isso aumenta o acoplamento e não atende à separação desejada.

## Consequências
A Lambda precisará de conectividade privada ao banco ou de uma API interna. O acesso será somente leitura e com menor privilégio.
