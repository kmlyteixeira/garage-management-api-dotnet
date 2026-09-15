# ADR-011: GitHub Actions

- Status: Proposed
- Data: 2026-09-08

## Contexto
O repositório central já usa GitHub Actions para build, testes, SonarQube, imagens e deploy.

## Decisão
Manter GitHub Actions com workflows independentes por repositório. Pull requests executam validações; deploy de homologação ocorre em `develop`; produção exige aprovação e merge em `main`.

## Alternativas consideradas
Jenkins e GitLab CI exigiriam uma plataforma adicional.

## Consequências
Branch protection, environments, OIDC AWS e secrets devem ser configurados manualmente no GitHub.
