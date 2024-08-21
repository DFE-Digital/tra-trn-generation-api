variable "environment_name" {
  type = string
}

variable "resource_prefix" {
  type    = string
  default = ""
}

variable "app_service_plan_sku_tier" {
  type    = string
  default = "Basic"
}

variable "storage_account" {
  type    = string
  default = ""
}

variable "app_service_plan_sku_size" {
  type    = string
  default = "B1"
}

variable "key_vault_name" {
  type = string
}

variable "azure_sp_credentials_json" {
  type    = string
  default = null
}

variable "resource_group_name" {
  type = string
}

variable "postgres_flexible_server_sku" {
  type    = string
  default = "B_Standard_B1ms"
}

variable "postgres_flexible_server_storage_mb" {
  type    = number
  default = 32768
}

variable "backup_container_delete_retention_days" {
  default = 7
  type    = number
}

variable "enable_postgres_high_availability" {
  type    = bool
  default = false
}

variable "enable_blue_green" {
  type    = bool
  default = false
}

variable "worker_count" {
  description = "It should be set to a multiple of the number of availability zones in the region"
  type        = number
  default     = null
}

variable "statuscake_alerts" {
  type    = map(any)
  default = {}
}

locals {
  hosting_environment    = var.environment_name
  app_service_plan_name  = "${var.resource_prefix}-trngen-${var.environment_name}-plan"
  postgres_server_name   = "${var.resource_prefix}-trngen-${var.environment_name}-psql"
  postgres_database_name = "trn_generation_api_production"
  web_app_name           = "${var.resource_prefix}-trngen-${var.environment_name}-app"
  web_app_insights_name  = "${var.resource_prefix}-trngen-${var.environment_name}-appi"
  web_app_slot_name      = "staging"
}
