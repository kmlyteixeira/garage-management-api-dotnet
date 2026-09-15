# RFC-001: Escolha da cloud

- Status: Proposed

## Problema
A solução precisa hospedar API, Lambda, gateway, Kubernetes, banco e observabilidade.

## Contexto
O código atual já usa AWS, Terraform, EKS, RDS e IAM.

## Alternativas
AWS, Azure e GCP.

## Critérios
Compatibilidade com o código existente, serviços gerenciados, IAM, custo, disponibilidade e experiência operacional.

## Solução proposta
AWS, inicialmente em `us-east-1`, mantendo Terraform como IaC.

## Impactos e riscos
Reduz migração, mas cria dependência da AWS. Região, orçamento, compliance e conta responsável ainda precisam ser confirmados.

## Plano
Separar Terraform de aplicação, banco e cluster; adotar OIDC do GitHub Actions e revisar rede privada.
