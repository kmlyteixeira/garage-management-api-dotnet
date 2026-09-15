# ADR-003: RDS PostgreSQL

- Status: Proposed
- Data: 2026-09-08

## Contexto
O domínio usa PostgreSQL 16, Npgsql e migrations EF Core. O Terraform atual já provisiona RDS PostgreSQL.

## Decisão
Usar Amazon RDS for PostgreSQL. O schema e as migrations permanecem na API; somente a infraestrutura fica em `garage-management-database`.

## Alternativas consideradas
Aurora PostgreSQL e banco autogerenciado. Aurora deve ser reavaliado se os requisitos de escala ou disponibilidade justificarem o custo.

## Consequências
Preservamos compatibilidade com o modelo atual. Subnets privadas, backups, retenção, Multi-AZ e proteção contra deleção precisam ser definidos por ambiente.
