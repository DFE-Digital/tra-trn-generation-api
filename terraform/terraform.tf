terraform {
  required_version = "1.3.4"

  backend "azurerm" {
    container_name = "trngen-tfstate"
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.32.0"
    }

    statuscake = {
      source  = "StatusCakeDev/statuscake"
      version = "2.0.5"
    }
  }
}
