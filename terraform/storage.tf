resource "azurerm_storage_account" "backup" {
  count                             = var.environment_name == "production" ? 1 : 0
  name                              = "s165p01trngenbackuppd"
  resource_group_name               = data.azurerm_resource_group.resource_group.name
  location                          = data.azurerm_resource_group.resource_group.location
  account_tier                      = "Standard"
  account_replication_type          = "GRS"
  account_kind                      = "StorageV2"
  min_tls_version                   = "TLS1_2"
  infrastructure_encryption_enabled = true

  blob_properties {
    last_access_time_enabled = true

    container_delete_retention_policy {
      days = var.backup_container_delete_retention_days
    }
  }

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_storage_container" "backup" {
  count                 = var.environment_name == "production" ? 1 : 0
  name                  = "trngen"
  storage_account_name  = "s165p01trngenbackuppd"
  container_access_type = "private"
}
