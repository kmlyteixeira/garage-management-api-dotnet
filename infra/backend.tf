# terraform {
#   backend "s3" {
#     bucket = "garage-management-api-terraform"
#     key    = "infra/terraform.tfstate"
#     region = "us-east-1"
#   }
# }

terraform {
  cloud {
    organization = "15soat-fiap"

    workspaces {
      name = "garage-management-api-dotnet"
    }
  }
}