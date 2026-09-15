# ADR-002: EKS como Kubernetes

- Status: Proposed
- Data: 2026-09-08

## Contexto
A aplicação já possui Deployment, Service, probes, HPA e Terraform para EKS.

## Decisão
Executar a API e o DbMigrator em Amazon EKS. O cluster será responsabilidade de `garage-management-infra`.

## Alternativas consideradas
ECS/Fargate reduziria a operação, mas não preservaria os manifests e o requisito Kubernetes. Kubernetes autogerenciado aumenta a carga operacional.

## Consequências
Mantemos portabilidade Kubernetes e HPA, assumindo custo e complexidade operacional do EKS.
