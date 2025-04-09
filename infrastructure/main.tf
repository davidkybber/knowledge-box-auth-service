# Reference existing resource group
data "azurerm_resource_group" "rg" {
  name = "knowledge-box-rg"
}

# Azure Container Registry
resource "azurerm_container_registry" "acr" {
  count               = var.acr_enabled ? 1 : 0
  name                = var.acr_name
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  sku                 = var.acr_sku
  admin_enabled       = var.acr_admin_enabled
  tags                = var.tags
}

# Container Apps Environment
resource "azurerm_container_app_environment" "env" {
  name                = "${var.project_name}-env"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  tags                = var.tags
}

# Container App for Auth Service
resource "azurerm_container_app" "auth_service" {
  name                         = "${var.project_name}-auth-service"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name         = data.azurerm_resource_group.rg.name
  revision_mode               = "Single"
  tags                        = var.tags

  identity {
    type = "SystemAssigned"
  }

  template {
    container {
      name   = "auth-service"
      image  = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "NODE_ENV"
        value = "production"
      }

      env {
        name  = "PORT"
        value = "3000"
      }

      # Add more environment variables as needed
    }
  }

  ingress {
    external_enabled = true
    target_port     = 3000
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
    allow_insecure_connections = false
  }
}

# Grant ACR pull access to the Container App's managed identity
resource "azurerm_role_assignment" "acr_pull" {
  count                = var.acr_enabled ? 1 : 0
  scope                = azurerm_container_registry.acr[0].id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_container_app.auth_service.identity[0].principal_id
}

# Grant ACR pull access to the deployment service principal
resource "azurerm_role_assignment" "acr_pull_deployment" {
  count                = var.acr_enabled ? 1 : 0
  scope                = azurerm_container_registry.acr[0].id
  role_definition_name = "AcrPull"
  principal_id         = var.deployment_sp_object_id
}

# Key Vault for secrets management (using free tier)
resource "azurerm_key_vault" "kv" {
  name                = "${var.project_name}-auth-kv"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  tenant_id          = data.azurerm_client_config.current.tenant_id
  sku_name           = "standard"
  tags                = var.tags
}

# Get current Azure configuration
data "azurerm_client_config" "current" {}

# Key Vault Access Policy
resource "azurerm_key_vault_access_policy" "kv_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get", "List", "Set", "Delete", "Purge"
  ]
}

# Key Vault Access Policy for Container App
resource "azurerm_key_vault_access_policy" "container_app_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_container_app.auth_service.identity[0].principal_id

  secret_permissions = [
    "Get", "List"
  ]
} 