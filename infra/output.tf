output "vpc_cidr" {
  value = aws_vpc.vpc_garage_management.cidr_block
}

output "vpc_id" {
  value = aws_vpc.vpc_garage_management.id
}

output "subnet_cidr" {
  value = aws_subnet.subnet_public[*].cidr_block
}

output "subnet_id" {
  value = aws_subnet.subnet_public[*].id
}

output "eks_cluster_name" {
  value = aws_eks_cluster.cluster.name
}

output "rds_endpoint" {
  value = aws_db_instance.postgres.address
}

output "rds_port" {
  value = aws_db_instance.postgres.port
}

output "rds_database_name" {
  value = aws_db_instance.postgres.db_name
}