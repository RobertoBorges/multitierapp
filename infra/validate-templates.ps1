# PowerShell script to validate Bicep templates
# This script performs validation on the Bicep files before deployment

# Set error action preference
$ErrorActionPreference = "Stop"

# Directory paths
$rootDir = $PSScriptRoot
$modulesDir = Join-Path -Path $rootDir -ChildPath "modules"

# Function to validate a Bicep file
function Test-BicepFile {
    param (
        [string]$filePath,
        [string]$displayName
    )
    
    Write-Host "`n▶️ Validating $displayName..." -ForegroundColor Cyan
    
    # Check if file exists
    if (-not (Test-Path -Path $filePath)) {
        Write-Host "❌ File not found: $filePath" -ForegroundColor Red
        return $false
    }
    
    # Run bicep build to check for syntax errors
    try {
        $output = az bicep build --file $filePath 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Bicep build failed for $displayName" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
            return $false
        }
        Write-Host "✅ $displayName syntax is valid" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Error validating $displayName: $_" -ForegroundColor Red
        return $false
    }
}

# Function to validate a template against a resource group
function Test-DeploymentValidation {
    param (
        [string]$templateFile,
        [string]$parametersFile,
        [string]$resourceGroupName = "validation-rg"
    )
    
    Write-Host "`n▶️ Performing deployment validation..." -ForegroundColor Cyan
    
    # Check if resource group exists, create if not
    $rgExists = az group exists --name $resourceGroupName --query exists -o tsv
    if ($rgExists -ne "true") {
        Write-Host "Creating temporary resource group '$resourceGroupName' for validation..."
        az group create --name $resourceGroupName --location eastus --output none
        $createdRg = $true
    }
    
    # Validate deployment
    try {
        $output = az deployment group validate `
            --resource-group $resourceGroupName `
            --template-file $templateFile `
            --parameters "@$parametersFile" 2>&1
            
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Deployment validation failed:" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
            return $false
        }
        
        Write-Host "✅ Deployment validation successful" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Error during deployment validation: $_" -ForegroundColor Red
        return $false
    }
    finally {
        # Clean up temporary resource group if we created it
        if ($createdRg) {
            Write-Host "Cleaning up temporary resource group..."
            az group delete --name $resourceGroupName --yes --no-wait
        }
    }
}

# Main validation process
Write-Host "🔍 Starting Bicep template validation..." -ForegroundColor Yellow

# Validate main template
$mainValid = Test-BicepFile -filePath (Join-Path -Path $rootDir -ChildPath "main.bicep") -displayName "Main template"

# Validate module templates
$modulesValid = $true
$moduleFiles = Get-ChildItem -Path $modulesDir -Filter "*.bicep"
foreach ($module in $moduleFiles) {
    $moduleValid = Test-BicepFile -filePath $module.FullName -displayName "Module: $($module.BaseName)"
    $modulesValid = $modulesValid -and $moduleValid
}

# Validate parameters file
$parametersFile = Join-Path -Path $rootDir -ChildPath "parameters-dev.json"
$parametersValid = Test-Path -Path $parametersFile
if (-not $parametersValid) {
    Write-Host "❌ Parameters file not found: $parametersFile" -ForegroundColor Red
}
else {
    Write-Host "✅ Parameters file exists" -ForegroundColor Green
}

# Perform deployment validation if all files are valid
$deploymentValid = $false
if ($mainValid -and $modulesValid -and $parametersValid) {
    $deploymentValid = Test-DeploymentValidation `
        -templateFile (Join-Path -Path $rootDir -ChildPath "main.bicep") `
        -parametersFile $parametersFile `
        -resourceGroupName "validation-rg"
}

# Summary
Write-Host "`n📋 Validation Summary:" -ForegroundColor Yellow
Write-Host "Main Template: $(if ($mainValid) { "✅ Valid" } else { "❌ Invalid" })"
Write-Host "Module Templates: $(if ($modulesValid) { "✅ Valid" } else { "❌ Invalid" })"
Write-Host "Parameters File: $(if ($parametersValid) { "✅ Valid" } else { "❌ Invalid" })"
Write-Host "Deployment Validation: $(if ($deploymentValid) { "✅ Valid" } else { "❌ Invalid" })"

# Overall result
if ($mainValid -and $modulesValid -and $parametersValid -and $deploymentValid) {
    Write-Host "`n✅ All validations passed! The infrastructure templates are ready for deployment." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n❌ Some validations failed. Please fix the issues before deploying." -ForegroundColor Red
    exit 1
}
