# PowerShell deployment script for BookShop Infrastructure
# This script deploys the Bicep templates to Azure and optionally publishes the application

param (
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus",

    [Parameter(Mandatory = $false)]
    [string]$Environment = "dev",

    [Parameter(Mandatory = $false)]
    [switch]$WhatIf,
    
    [Parameter(Mandatory = $false)]
    [switch]$PublishApp
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

# Check if custom parameters file exists
$customParametersFile = "parameters-custom.json"
if (Test-Path -Path $customParametersFile) {
    $parametersFile = $customParametersFile
    Write-Host "Using custom parameters file: $parametersFile"
}

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
    
    # If not publishing, show hint about the -PublishApp parameter
    if (-not $PublishApp) {
        Write-Host "`nTip: To publish the application code to this App Service, run the script with the -PublishApp parameter:"
        Write-Host "     .\deploy.ps1 -ResourceGroupName $ResourceGroupName -Environment $Environment -PublishApp"
    }
    
    # Store the App Service name for later use if publishing
    $appServiceName = $outputs.appServiceName.value
    
    # If PublishApp switch is provided, publish and deploy the application code
    if ($PublishApp) {
        $webProjectPath = Join-Path (Split-Path -Parent $PSScriptRoot) "src\BookShop.Web"
        
        Write-Host "`nPublishing application from $webProjectPath..."
        
        # Navigate to the web project directory
        Push-Location $webProjectPath
        
        try {
            # Build and publish the application
            Write-Host "Building and publishing application..."
            dotnet publish -c Release
            
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Failed to publish the application"
                Pop-Location
                exit 1
            }
            
            # Create a zip file from the published output
            $publishFolder = Join-Path $webProjectPath "bin\Release\net8.0\publish"
            $zipPath = Join-Path $env:TEMP "bookshop-app.zip"
            
            if (Test-Path $zipPath) {
                Remove-Item $zipPath -Force
            }
            
            Write-Host "Creating deployment package..."
            Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipPath
            
            # Deploy the zip package to Azure App Service
            Write-Host "Deploying application to Azure App Service..."
            az webapp deployment source config-zip --src $zipPath --name $appServiceName --resource-group $ResourceGroupName
            
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Failed to deploy the application to Azure App Service"
                Pop-Location
                exit 1
            }
            
            Write-Host "Application successfully published and deployed to Azure App Service!"
            Write-Host "You can access the application at: $($outputs.appServiceUrl.value)"
        }
        finally {
            # Ensure we return to the original directory
            Pop-Location
        }
    }
} elseif ($LASTEXITCODE -ne 0 -and -not $WhatIf) {
    Write-Error "Deployment failed."
}
