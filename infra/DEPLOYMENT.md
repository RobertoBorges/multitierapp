# BookShop Azure Deployment Guide

This guide provides step-by-step instructions for deploying the modernized BookShop application to Azure using the infrastructure templates provided in this repository.

## Prerequisites

- Azure CLI installed and configured
- PowerShell 7.0 or later
- Access to an Azure subscription with contributor permissions
- Git repository cloned locally

## Step 1: Validate Infrastructure Templates

Before deployment, validate the infrastructure templates to ensure they are correctly formatted and will deploy successfully:

```powershell
cd infra
./validate-templates.ps1
```

Fix any issues reported by the validation script before proceeding.

## Step 2: Create Resource Group

Create a resource group for your BookShop application:

```powershell
az group create --name bookshop-rg --location eastus
```

## Step 3: Deploy Infrastructure

Deploy the infrastructure using the Azure Bicep templates:

```powershell
cd infra
./deploy.ps1 -ResourceGroupName "bookshop-rg" -Environment "dev"
```

This will provision:
- App Service Plan
- App Service
- Application Insights
- Log Analytics Workspace
- Key Vault
- Backup storage configuration

## Step 4: Configure Entra ID Authentication

Set up Entra ID (Azure AD) authentication for your application:

```powershell
# Get the App Service URL from the deployment output
$appServiceUrl = "https://bookshop-app-dev.azurewebsites.net" # Replace with your actual URL

# Configure Entra ID
./configure-entra-id.ps1 -AppName "BookShop" -AppServiceUrl $appServiceUrl
```

Note the output values for `ClientId` and `TenantId` as you'll need them to configure your application.

## Step 5: Update Application Settings

Add the application settings to your App Service:

```powershell
az webapp config appsettings set --name bookshop-app-dev --resource-group bookshop-rg --settings "ASPNETCORE_ENVIRONMENT=Development" "AzureAd:TenantId=your-tenant-id" "AzureAd:ClientId=your-client-id"
```

## Step 6: Set Up Database Connection

Retrieve the connection string from Key Vault and configure it in the App Service:

```powershell
$connectionStringSecret = "SqlConnectionString" # Secret name in Key Vault
$keyVaultName = "bookshop-kv-dev123456789" # Replace with your Key Vault name

# Get connection string from Key Vault
$connectionString = az keyvault secret show --name $connectionStringSecret --vault-name $keyVaultName --query value -o tsv

# Set connection string in App Service
az webapp config connection-string set --name bookshop-app-dev --resource-group bookshop-rg --connection-string-type SQLAzure --settings DefaultConnection=$connectionString
```

## Step 7: Deploy Application Code

Deploy your application code to the App Service using Azure CLI or GitHub Actions:

```powershell
# Using Azure CLI
cd src/BookShop.Web
dotnet publish -c Release
cd bin/Release/net8.0/publish
az webapp deployment source config-zip --src ./site.zip --name bookshop-app-dev --resource-group bookshop-rg
```

## Step 8: Verify Deployment

1. Browse to your App Service URL: `https://bookshop-app-dev.azurewebsites.net`
2. Verify that the application loads correctly
3. Test authentication using Entra ID
4. Verify that database connectivity works by performing CRUD operations
5. Check that Application Insights is receiving telemetry data

## Troubleshooting

If you encounter issues:

1. **App Service shows deployment errors**:
   - Check the deployment logs in the Azure Portal
   - Verify that the build process completed successfully

2. **Authentication issues**:
   - Ensure the redirect URI is correctly configured in Entra ID
   - Check the application settings for correct TenantId and ClientId values

3. **Database connection issues**:
   - Verify the connection string in App Service configuration
   - Check that the managed identity has appropriate permissions on the database

4. **Application Insights not showing data**:
   - Ensure the instrumentation key is correctly configured
   - Check that the SDK is properly initialized in the application

## Next Steps

After deployment:
1. Set up CI/CD pipelines using GitHub Actions or Azure DevOps
2. Configure additional monitoring and alerting
3. Set up staging slots for blue-green deployments
4. Implement automated testing as part of the deployment process
