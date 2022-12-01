output "service_plan_id" {
  value = azurerm_service_plan.app_service_plan.id
}

output "web_app_name" {
  value = local.web_app_name
}

output "web_app_slot_name" {
  value = var.enable_blue_green ? local.web_app_slot_name : "production"
}

output "blue_green" {
  value = var.enable_blue_green
}

output "service_sku_size" {
  value = var.app_service_plan_sku_size
  }
