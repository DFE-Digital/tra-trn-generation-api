output "service_plan_id" {
  value = azurerm_service_plan.app_service_plan.id
}

output "web_app_name" {
  value = local.web_app_name
}
