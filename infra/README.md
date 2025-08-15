# BookShop Application Infrastructure

This directory contains the Azure Bicep templates for deploying the BookShop application infrastructure to Azure.

## Infrastructure Components

- **Azure App Service**: Hosts the ASP.NET Core MVC application
- **Azure App Service Plan**: Provides the compute resources for the App Service
- **Application Insights**: Monitors application performance and usage
- **Log Analytics Workspace**: Centralizes logs from all services
- **Key Vault**: Securely stores application secrets and connection strings

## Prerequisites

- Azure CLI installed
- Azure subscription
- Permission to create resources in Azure

## Deployment Instructions

### 1. Login to Azure

```bash
az login
```

### 2. Set your subscription

```bash
az account set --subscription "Your Subscription Name or ID"
```

### 3. Deploy using the script

```bash
./deploy.ps1 -ResourceGroupName "bookshop-rg" -Environment "dev"
```

Available parameters:
- `-ResourceGroupName`: Name of the resource group to deploy to (required)
- `-Location`: Azure region for deployment (default: "eastus")
- `-Environment`: Environment name - dev, test, or prod (default: "dev")
- `-WhatIf`: Preview changes without deploying (optional)

### 4. Manual Deployment

Alternatively, you can deploy manually using the Azure CLI:

```bash
az group create --name bookshop-rg --location eastus
az deployment group create --resource-group bookshop-rg --template-file main.bicep --parameters @parameters-dev.json
```

## Environment-specific Parameters

The deployment uses environment-specific parameter files:
- `parameters-dev.json`: Development environment
- `parameters-test.json`: Testing environment
- `parameters-prod.json`: Production environment

## Security Notes

- The App Service is configured with HTTPS only
- Modern TLS 1.2 is enforced
- Key Vault is used for secret management
- System-assigned managed identity is enabled for the App Service

## Post-Deployment Steps

After deployment:
1. Configure App Service with proper connection strings from Key Vault
2. Set up Entra ID (Azure AD) application for authentication
3. Configure appropriate CORS settings if needed
4. Set up CI/CD pipeline for automated deployments
5. Review and optimize App Service settings for your specific requirements
