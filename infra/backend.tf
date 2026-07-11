terraform {
  backend "s3" {
    bucket = "garage-management-api-terraform"
    key    = "infra/terraform.tfstate"
    region = "us-east-1"
  }
}