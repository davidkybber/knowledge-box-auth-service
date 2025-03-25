# Configure the Azure provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "knowledgeboxstate"
    container_name       = "auth-service-tfstate"
    key                  = "knowledge-box.terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
} 