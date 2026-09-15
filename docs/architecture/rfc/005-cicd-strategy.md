# RFC-005: Estratégia de CI/CD

- Status: Proposed

## Problema
O pipeline atual centraliza aplicação, Terraform e deploy, mas o alvo exige quatro repositórios independentes.

## Contexto
Já existe GitHub Actions com build, testes, SonarQube, GHCR e deploy AWS/Kubernetes.

## Alternativas
GitHub Actions, Jenkins e GitLab CI.

## Critérios
Integração com GitHub, ambientes protegidos, OIDC AWS, revisão, artefatos e rollback.

## Solução proposta
GitHub Actions independente por repositório: CI em pull request; homologação em `develop`; produção em `main` com environment protegido e aprovação.

## Impactos e riscos
Workflows ficam desacoplados e exigem contratos de versão entre imagens, Terraform e API. OIDC deve substituir credenciais AWS estáticas.

## Plano
Criar workflows por repositório, configurar branch protection/environments, validar plan, testes, scans, assinatura de imagem e deploy progressivo.
