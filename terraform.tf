provider "azurerm" {
  subscription_id = "16ede44b-1f74-40a5-b428-46cca9a5741b"
  features {}
}

locals {
  location            = "North Europe"
  resource_group_name = "test-resources"

  name            = "rihag-deleteme-edc23"
  queue_name      = "orders"
  managed_id_name = "radix-sample-keda"

  radix_app_name        = "edc2023-radix-wi-rihag"
  radix_app_env         = "prod"
  radix_oidc_issuer_url = "https://northeurope.oic.prod-aks.azure.com/3aa4a235-b6e2-48d5-9195-7fcf05b459b0/04f44167-e128-4c51-a26c-2589bd33b7ac/"
}

output "client_id" {
  value = azurerm_user_assigned_identity.main.client_id
}
output "endpoint" {
  value = azurerm_servicebus_namespace.main.endpoint
}
output "queue_name" {
  value = local.queue_name
}


resource "azurerm_servicebus_namespace" "main" {
  location            = local.location
  name                = local.name
  resource_group_name = local.resource_group_name
  sku                 = "Basic"
}

resource "azurerm_servicebus_queue" "main" {
  name         = local.queue_name
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_user_assigned_identity" "main" {
  name                = local.managed_id_name
  location            = azurerm_servicebus_namespace.main.location
  resource_group_name = azurerm_servicebus_namespace.main.resource_group_name
}

resource "azurerm_role_assignment" "main" {
  principal_id         = azurerm_user_assigned_identity.main.principal_id
  scope                = azurerm_servicebus_namespace.main.id
  role_definition_name = "Azure Service Bus Data Owner"
}

resource "azurerm_federated_identity_credential" "keda" {
  audience            = ["api://AzureADTokenExchange"]
  issuer              = local.radix_oidc_issuer_url
  name                = "keda"
  resource_group_name = azurerm_servicebus_namespace.main.resource_group_name
  subject             = "system:serviceaccount:keda:keda-operator"
  parent_id           = azurerm_user_assigned_identity.main.id
}

resource "azurerm_federated_identity_credential" "web" {
  audience            = ["api://AzureADTokenExchange"]
  issuer              = local.radix_oidc_issuer_url
  name                = "web"
  resource_group_name = azurerm_servicebus_namespace.main.resource_group_name
  subject             = "system:serviceaccount:${local.radix_app_name}-${local.radix_app_env}:web-sa"
  parent_id           = azurerm_user_assigned_identity.main.id
}

resource "azurerm_federated_identity_credential" "processor" {
  audience            = ["api://AzureADTokenExchange"]
  issuer              = local.radix_oidc_issuer_url
  name                = "processor"
  resource_group_name = azurerm_servicebus_namespace.main.resource_group_name
  subject             = "system:serviceaccount:${local.radix_app_name}-${local.radix_app_env}:processor-sa"
  parent_id           = azurerm_user_assigned_identity.main.id
}
