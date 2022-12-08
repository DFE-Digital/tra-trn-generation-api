locals {
  trngen_env_vars = {
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.web_app_insights.connection_string
    Serilog__WriteTo__2__Args__uri        = local.infrastructure_secrets.LOGSTASH_ENDPOINT
    ConnectionStrings__DefaultConnection  = "Server=${local.postgres_server_name}.postgres.database.azure.com;User Id=${local.infrastructure_secrets.POSTGRES_ADMIN_USERNAME};Password=${local.infrastructure_secrets.POSTGRES_ADMIN_PASSWORD};Database=${local.postgres_database_name};Port=5432;Trust Server Certificate=true;"
    ApiKeys__0                            = local.infrastructure_secrets.ApiKeys__0
    ApiKeys__1                            = local.infrastructure_secrets.ApiKeys__1
    Sentry__Dsn                           = local.infrastructure_secrets.SENTRY_DSN
  }
}

resource "azurerm_service_plan" "app_service_plan" {
  name                   = local.app_service_plan_name
  location               = data.azurerm_resource_group.resource_group.location
  resource_group_name    = data.azurerm_resource_group.resource_group.name
  os_type                = "Linux"
  sku_name               = var.app_service_plan_sku_size
  zone_balancing_enabled = var.worker_count != null ? true : false
  worker_count           = var.worker_count

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_linux_web_app" "web_app" {
  name                = local.web_app_name
  location            = data.azurerm_resource_group.resource_group.location
  resource_group_name = data.azurerm_resource_group.resource_group.name
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  https_only          = true
  dynamic "sticky_settings" {
    for_each = var.enable_blue_green ? [1] : []
    content {
      app_setting_names = keys(data.azurerm_linux_web_app.web_app[0].app_settings)
    }
  }

  app_settings = var.enable_blue_green ? data.azurerm_linux_web_app.web_app[0].app_settings : local.trngen_env_vars

  identity {
    type = "SystemAssigned"
  }

  site_config {
    http2_enabled       = true
    minimum_tls_version = "1.2"
    health_check_path   = "/health"
  }
}

resource "azurerm_linux_web_app_slot" "web_app_slot" {
  count          = var.enable_blue_green ? 1 : 0
  name           = local.web_app_slot_name
  app_service_id = azurerm_linux_web_app.web_app.id

  site_config {
    http2_enabled       = true
    minimum_tls_version = "1.2"
    health_check_path   = "/health"
  }
  depends_on = [
    azurerm_service_plan.app_service_plan
  ]

  app_settings = local.trngen_env_vars

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_postgresql_flexible_server" "postgres-server" {
  name                   = local.postgres_server_name
  location               = data.azurerm_resource_group.resource_group.location
  resource_group_name    = data.azurerm_resource_group.resource_group.name
  version                = "14"
  administrator_login    = local.infrastructure_secrets.POSTGRES_ADMIN_USERNAME
  administrator_password = local.infrastructure_secrets.POSTGRES_ADMIN_PASSWORD
  create_mode            = "Default"
  storage_mb             = var.postgres_flexible_server_storage_mb
  sku_name               = var.postgres_flexible_server_sku

  dynamic "high_availability" {
    for_each = var.enable_postgres_high_availability ? [1] : []

    content {
      mode = "ZoneRedundant"
    }
  }

  lifecycle {
    ignore_changes = [
      tags,
      # Allow Azure to manage deployment zone. Ignore changes.
      zone,
      # Allow Azure to manage primary and standby server on fail-over. Ignore changes.
      high_availability[0].standby_availability_zone
    ]
  }
}

resource "azurerm_postgresql_flexible_server_database" "postgres-database" {
  name      = local.postgres_database_name
  server_id = azurerm_postgresql_flexible_server.postgres-server.id
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "postgres-fw-rule-azure" {
  name             = "AllowAzure"
  server_id        = azurerm_postgresql_flexible_server.postgres-server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_application_insights" "web_app_insights" {
  name                = local.web_app_insights_name
  resource_group_name = data.azurerm_resource_group.resource_group.name
  location            = data.azurerm_resource_group.resource_group.location
  application_type    = "web"
}
