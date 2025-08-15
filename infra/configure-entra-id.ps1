# Configure Entra ID Authentication for BookShop Application
# This script helps set up Entra ID (Azure AD) app registration for authentication

param (
    [Parameter(Mandatory = $true)]
    [string]$AppName = "BookShop",
    
    [Parameter(Mandatory = $true)]
    [string]$AppServiceUrl,
    
    [Parameter(Mandatory = $false)]
    [string]$IdentifierUri
)

# Ensure we're logged in
$account = az account show --query name -o tsv 2>$null
if (-not $account) {
    Write-Error "Not logged in to Azure. Please run 'az login' first."
    exit 1
}

Write-Host "Setting up Entra ID app registration for $AppName..."

# Create the app registration if it doesn't exist
$appId = az ad app list --display-name $AppName --query "[0].appId" -o tsv
if (-not $appId) {
    # Set redirect URIs
    $redirectUris = @(
        "$AppServiceUrl",
        "$AppServiceUrl/",
        "$AppServiceUrl/signin-oidc"
    )
    
    # Create app registration
    $appInfo = az ad app create --display-name $AppName `
        --sign-in-audience "AzureADMyOrg" `
        --web-redirect-uris $redirectUris `
        | ConvertFrom-Json
    
    $appId = $appInfo.appId
    
    Write-Host "Created new app registration with ID: $appId"
} else {
    Write-Host "Found existing app registration with ID: $appId"
    
    # Update redirect URIs
    az ad app update --id $appId `
        --web-redirect-uris "$AppServiceUrl" "$AppServiceUrl/" "$AppServiceUrl/signin-oidc"
}

# Add API permissions for Microsoft Graph (optional)
# az ad app permission add --id $appId --api 00000003-0000-0000-c000-000000000000 --api-permissions "e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope"
# az ad app permission grant --id $appId --api 00000003-0000-0000-c000-000000000000

# Get the tenant ID
$tenantId = az account show --query tenantId -o tsv

Write-Host "`nEntra ID Setup Complete!"
Write-Host "==============================================="
Write-Host "Application (client) ID: $appId"
Write-Host "Directory (tenant) ID: $tenantId"
Write-Host "Redirect URIs configured to: $AppServiceUrl/signin-oidc"
Write-Host "`nNext Steps:"
Write-Host "1. Update your application's appsettings.json with these values"
Write-Host "2. Create a client secret if needed (for confidential clients)"
Write-Host "3. Add appropriate API permissions if required"

# Output example configuration
Write-Host "`nExample appsettings.json configuration:"
Write-Host "-----------------------------------------------"
$configExample = @"
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "$($tenantId).onmicrosoft.com",
  "TenantId": "$tenantId",
  "ClientId": "$appId",
  "CallbackPath": "/signin-oidc"
}
"@

Write-Host $configExample
