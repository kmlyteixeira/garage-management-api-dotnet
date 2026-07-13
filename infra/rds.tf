resource "aws_db_subnet_group" "db_subnet_group" {
  name       = "${var.projectName}-db"
  subnet_ids = aws_subnet.subnet_public[*].id
  tags       = var.tags
}

resource "aws_security_group" "db_sg" {
  name        = "${var.projectName}-db-sg"
  description = "Allow Postgres access from the EKS cluster only"
  vpc_id      = aws_vpc.vpc_garage_management.id

  ingress {
    description     = "Postgres from EKS cluster/nodes"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.sg.id]
  }

  ingress {
    description = "Postgres from within the VPC"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = [var.cidr_vpc]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = var.tags
}

resource "aws_db_instance" "postgres" {
  identifier              = "${var.projectName}-db"
  engine                  = "postgres"
  engine_version          = "16"
  instance_class          = var.db_instance_class
  allocated_storage       = 20
  storage_type            = "gp3"
  storage_encrypted       = true
  db_name                 = var.db_name
  username                = var.db_username
  password                = var.db_password
  db_subnet_group_name    = aws_db_subnet_group.db_subnet_group.name
  vpc_security_group_ids  = [aws_security_group.db_sg.id]
  publicly_accessible     = false
  multi_az                = false
  skip_final_snapshot     = true
  deletion_protection     = false
  backup_retention_period = 1

  tags = var.tags
}
