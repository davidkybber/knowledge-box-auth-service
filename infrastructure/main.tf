# Reference existing resource group
data "azurerm_resource_group" "rg" {
  name = "knowledge-box-rg"
}

# Azure Container Registry
resource "azurerm_container_registry" "acr" {
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

  template {
    container {
      name   = "auth-service"
      image  = "${azurerm_container_registry.acr.login_server}/${var.project_name}-auth-service:latest"
      cpu    = 0.5
      memory = "1Gi"

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

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "registry-password"
  }

  secret {
    name  = "registry-password"
    value = azurerm_container_registry.acr.admin_password
  }
}

# Application Insights for monitoring
resource "azurerm_application_insights" "appinsights" {
  name                = "${var.project_name}-auth-insights"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  application_type    = "web"
  tags                = var.tags
}

# Key Vault for secrets management
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

# Associate access policies with Key Vault
resource "azurerm_key_vault_policy_assignment" "kv_policy" {
  key_vault_id = azurerm_key_vault.kv.id
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id
    secret_permissions = [
      "Get", "List", "Set", "Delete", "Purge"
    ]
  }
} 