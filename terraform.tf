provider "azurerm" {
  subscription_id = "00000000-0000-0000-0000-000000000000" # FIXME
  features {}
}

locals {
  location            = "North Europe"   #FIXME
  resource_group_name = "test-resources" #FIXME

  name            = "<Your service bus namespace>" #FIXME
  queue_name      = "orders"
  managed_id_name = "radix-sample-keda"

  radix_app_name = "radix-keda-sample"
  radix_app_env  = "prod"

  # https://console.radix.equinor.com/about
  radix_oidc_issuer_url = "https://northeurope.oic.prod-aks.azure.com/00000000-0000-0000-0000-000000000000/00000000-0000-0000-0000-000000000000/" #FIXME
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

### Set up Service Bus and queue

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

### Configure User Assigned Identity, Role Assignments and federated credentials

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

# Add access for Keda Operator to access the queue
resource "azurerm_federated_identity_credential" "keda" {
  audience            = ["api://AzureADTokenExchange"]
  issuer              = local.radix_oidc_issuer_url
  name                = "keda"
  resource_group_name = azurerm_servicebus_namespace.main.resource_group_name
  subject             = "system:serviceaccount:keda:keda-operator"
  parent_id           = azurerm_user_assigned_identity.main.id
}
