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