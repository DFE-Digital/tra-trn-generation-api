data "azurerm_resource_group" "resource_group" {
  name = var.resource_group_name
}

data "azurerm_key_vault" "vault" {
  name                = var.key_vault_name
  resource_group_name = data.azurerm_resource_group.resource_group.name
}

data "azurerm_key_vault_secrets" "secrets" {
  key_vault_id = data.azurerm_key_vault.vault.id
}

data "azurerm_key_vault_secret" "secrets" {
  for_each     = toset(data.azurerm_key_vault_secrets.secrets.names)
  key_vault_id = data.azurerm_key_vault.vault.id
  name         = each.key
}
