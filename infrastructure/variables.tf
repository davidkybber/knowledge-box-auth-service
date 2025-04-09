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

variable "acr_enabled" {
  description = "Enable or disable the ACR (to save costs when not in use)"
  type        = bool
  default     = true
}

variable "deployment_sp_object_id" {
  description = "Object ID of the deployment service principal used by GitHub Actions"
  type        = string
} 