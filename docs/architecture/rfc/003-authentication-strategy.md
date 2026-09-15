# RFC-003: Estratégia de autenticação

- Status: Proposed

## Problema
O requisito exige CPF, cliente ativo e JWT emitido por Lambda, enquanto a API atual usa OpenIddict.

## Contexto
A Lambda-fonte não existe atualmente. A API possui autoridade OpenIddict e password grant.

O agregado `Customer` atual contém nome, e-mail, telefone e documento, mas não contém status de ativo/inativo.

## Alternativas
Manter OpenIddict, usar Cognito ou criar Lambda emissora.

## Critérios
Compatibilidade com o requisito, segurança, rotação de chaves, operação e não duplicação de regras.

## Solução proposta
Lambda autentica e emite JWT assinado assimetricamente. API Gateway valida via authorizer JWT quando o JWKS estiver disponível; a API valida novamente issuer, audience, assinatura, expiração e claims. OpenIddict será descontinuado após uma transição validada.

## Impactos e riscos
Exige contrato de claims, endpoint JWKS, gestão de chaves e acesso de leitura ao cliente. A transição precisa evitar dois emissores aceitos indefinidamente.

A regra de bloqueio de cliente inativo não pode ser implementada corretamente até que a fonte do status seja definida.

## Plano
Implementar contrato/testes, provisionar Secrets Manager/KMS, publicar JWKS, adicionar validação na API e remover gradualmente o password grant.
