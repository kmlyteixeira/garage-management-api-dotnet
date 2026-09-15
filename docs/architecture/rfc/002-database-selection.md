# RFC-002: Escolha do banco

- Status: Proposed

## Problema
A aplicação precisa manter o modelo relacional atual com migrations EF Core.

## Contexto
O projeto usa PostgreSQL 16, Npgsql e RDS PostgreSQL já definido no Terraform.

## Alternativas
RDS PostgreSQL, Aurora PostgreSQL e PostgreSQL autogerenciado.

## Critérios
Compatibilidade, custo, backup, disponibilidade, operação e migração sem alteração do schema.

## Solução proposta
RDS PostgreSQL. Migrations permanecem no repositório da API e são executadas pelo DbMigrator controlado.

## Impactos e riscos
A compatibilidade é alta. O desenho atual precisa corrigir subnets públicas, retenção baixa, Single-AZ e proteção contra deleção.

## Plano
Extrair a infraestrutura para `garage-management-database`, parametrizar ambientes e validar restore antes do deploy.
