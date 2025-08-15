# PowerShell deployment script for BookShop Infrastructure
# This script deploys the Bicep templates to Azure

param (
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $false)]
    [string]$Environment = "dev",

    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

# Check if the resource group exists
$resourceGroup = az group show --name $ResourceGroupName --query name -o tsv --only-show-errors 2>$null
if (-not $resourceGroup) {
    Write-Host "Resource group '$ResourceGroupName' does not exist. Creating..."
    az group create --name $ResourceGroupName --location $Location
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create resource group '$ResourceGroupName'"
        exit 1
    }
}

# Deploy the Bicep template
$parametersFile = "parameters-$Environment.json"
$deploymentName = "bookshop-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

Write-Host "Deploying BookShop infrastructure to '$ResourceGroupName' using '$parametersFile'..."

if ($WhatIf) {
    az deployment group what-if --resource-group $ResourceGroupName `
        --name $deploymentName `
        --template-file main.bicep `
        --parameters @$parametersFile
} else {
    az deployment group create --resource-group $ResourceGroupName `
        --name $deploymentName `
        --template-file main.bicep `
        --parameters @$parametersFile
}

if ($LASTEXITCODE -eq 0 -and -not $WhatIf) {
    Write-Host "Deployment completed successfully!"

    # Get outputs
    $outputs = az deployment group show --resource-group $ResourceGroupName --name $deploymentName --query properties.outputs -o json | ConvertFrom-Json
    
    Write-Host "`nApplication URL: $($outputs.appServiceUrl.value)"
    Write-Host "Key Vault URI: $($outputs.keyVaultUri.value)"
} elseif ($LASTEXITCODE -ne 0 -and -not $WhatIf) {
    Write-Error "Deployment failed."
}
