# ADR-004: Autenticação por CPF e JWT

- Status: Proposed
- Data: 2026-09-08

## Contexto
O requisito define CPF como entrada, validação de cliente ativo e emissão de JWT. A API atual usa OpenIddict local.

O modelo atual de `Customer` não possui campo de status; portanto, a regra de cliente ativo ainda não tem uma fonte de dados definida.

## Decisão
A Lambda será a única responsável por autenticar CPF e emitir JWT. O token terá issuer, audience, expiração curta, subject do cliente e claims mínimas necessárias. A API validará assinatura, issuer, audience, expiração e claims como defesa em profundidade.

## Alternativas consideradas
Manter password grant/OpenIddict ou usar Cognito. Ambas não atendem diretamente ao fluxo CPF + Lambda definido.

## Consequências
Será necessária migração controlada do OpenIddict atual. Chaves privadas ficam em Secrets Manager/KMS; a chave pública/JWKS será publicada de forma segura para os validadores.

A implementação da Lambda fica bloqueada até definir se o status será adicionado ao modelo `Customer` ou obtido de outra fonte oficial.
