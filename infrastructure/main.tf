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

# Generate secure random JWT key
resource "random_password" "jwt_key" {
  length           = 64
  special          = true
  override_special = "!@#$%&*()-_=+[]{}<>:?"
}

# Generate secure random PostgreSQL password
resource "random_password" "postgres_password" {
  length           = 32
  special          = true
  override_special = "!@#$%&*()-_=+[]{}<>:?"
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
        value = "8080"
      }

      # JWT Authentication settings
      env {
        name  = "Jwt__Key"
        secret_name = "jwt-key"
      }

      env {
        name  = "Jwt__Issuer"
        value = var.jwt_issuer
      }

      env {
        name  = "Jwt__Audience"
        value = var.jwt_audience
      }

      env {
        name  = "Jwt__DurationInMinutes"
        value = var.jwt_duration_minutes
      }

      # Database connection string
      env {
        name  = "ConnectionStrings__DefaultConnection"
        secret_name = "db-connection-string"
      }
    }
  }

  secret {
    name  = "acr-password"
    value = var.acr_admin_enabled ? azurerm_container_registry.acr[0].admin_password : null
  }

  secret {
    name  = "jwt-key"
    value = random_password.jwt_key.result
  }

  secret {
    name  = "db-connection-string"
    value = "Host=${var.postgres_host};Port=${var.postgres_port};Database=${var.postgres_db};Username=${var.postgres_user};Password=${random_password.postgres_password.result};Pooling=true;"
  }

  registry {
    server   = "${var.acr_name}.azurecr.io"
    username = var.acr_admin_enabled ? var.acr_name : null
    password_secret_name = var.acr_admin_enabled ? "acr-password" : null
  }

  ingress {
    external_enabled = true
    target_port     = 8080
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

# Store JWT key in Key Vault
resource "azurerm_key_vault_secret" "jwt_key" {
  name         = "jwt-key"
  value        = random_password.jwt_key.result
  key_vault_id = azurerm_key_vault.kv.id
  depends_on   = [azurerm_key_vault_access_policy.kv_access]
}

# Store DB connection string in Key Vault
resource "azurerm_key_vault_secret" "db_connection_string" {
  name         = "db-connection-string"
  value        = "Host=${var.postgres_host};Port=${var.postgres_port};Database=${var.postgres_db};Username=${var.postgres_user};Password=${random_password.postgres_password.result};Pooling=true;"
  key_vault_id = azurerm_key_vault.kv.id
  depends_on   = [azurerm_key_vault_access_policy.kv_access]
} 