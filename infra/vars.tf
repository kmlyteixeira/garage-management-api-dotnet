variable "projectName" {
  default = "garage-management-api-terraform"
}

variable "region_default" {
  default = "us-east-1"
}

variable "cidr_vpc" {
  default = "10.0.0.0/16"
}

variable "tags" {
  default = {
    Name = "garage-management-api-terraform"
  }
}

variable "instance_type" {
  default = "t3.medium"
}

variable "db_instance_class" {
  default = "db.t3.micro"
}

variable "db_name" {
  default = "GarageManagement"
}

variable "db_username" {
  default = "garage_user"
}

variable "db_password" {
  description = "Postgres master password. Must be provided via TF_VAR_db_password, never committed."
  type        = string
  sensitive   = true
}