# ADR-001: AWS como cloud

- Status: Proposed
- Data: 2026-09-08

## Contexto
A solução já possui Terraform funcional para AWS, EKS, RDS, IAM, VPC e security groups. A documentação e os manifestos também usam serviços AWS.

## Decisão
Adotar AWS como cloud de referência.

## Alternativas consideradas
Azure e GCP exigiriam substituir a infraestrutura existente, os serviços gerenciados e parte do pipeline.

## Consequências
Reduzimos retrabalho e preservamos o investimento atual. A decisão depende de conta, região, custos e requisitos de compliance ainda não confirmados.
