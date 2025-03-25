variable "project_name" {
  description = "Name of the project"
  type        = string
  default     = "knowledge-box"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    environment = "production"
    project     = "knowledge-box"
    managed_by  = "terraform"
  }
}

variable "container_registry_url" {
  description = "URL of the container registry"
  type        = string
}

variable "container_registry_username" {
  description = "Username for the container registry"
  type        = string
}

variable "container_registry_password" {
  description = "Password for the container registry"
  type        = string
  sensitive   = true
}

variable "acr_name" {
  description = "Name of the Azure Container Registry"
  type        = string
  default     = "knowledgeboxacr"
}

variable "acr_sku" {
  description = "SKU of the Azure Container Registry"
  type        = string
  default     = "Basic"
}

variable "acr_admin_enabled" {
  description = "Enable admin user for ACR"
  type        = bool
  default     = true
} 