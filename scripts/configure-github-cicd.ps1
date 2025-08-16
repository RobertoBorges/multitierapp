# PowerShell script to configure GitHub repository settings for CI/CD
# This script assists in setting up GitHub repository settings and creating an Azure Service Principal for CI/CD

param (
    [Parameter(Mandatory = $true)]
    [string]$GitHubRepoOwner,
    
    [Parameter(Mandatory = $true)]
    [string]$GitHubRepoName,
    
    [Parameter(Mandatory = $true)]
    [string]$GitHubToken,
    
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory = $true)]
    [string]$DevResourceGroupName,
    
    [Parameter(Mandatory = $true)]
    [string]$TestResourceGroupName,
    
    [Parameter(Mandatory = $true)]
    [string]$ProdResourceGroupName
)

Write-Host "Configuring GitHub repository settings for CI/CD..."

# 1. Create Azure Service Principal for GitHub Actions
Write-Host "Creating Azure Service Principal for GitHub Actions..."

$servicePrincipalName = "github-actions-bookshop"
$servicePrincipal = az ad sp create-for-rbac --name $servicePrincipalName --role contributor --scopes /subscriptions/$SubscriptionId/resourceGroups/$DevResourceGroupName /subscriptions/$SubscriptionId/resourceGroups/$TestResourceGroupName /subscriptions/$SubscriptionId/resourceGroups/$ProdResourceGroupName --sdk-auth -o json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create service principal"
    exit 1
}

$servicePrincipalJson = $servicePrincipal | ConvertFrom-Json

# 2. Create GitHub Environments
Write-Host "Creating GitHub Environments..."

# Create Development Environment
$devEnvironmentData = @{
    name = "development"
    deployment_branch_policy = @{
        protected_branches = $false
        custom_branch_policies = $true
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/environments/development" `
    -Method PUT `
    -Headers @{
        Authorization = "token $GitHubToken"
        "Content-Type" = "application/json"
        Accept = "application/vnd.github.v3+json"
    } `
    -Body $devEnvironmentData

# Create Test Environment with approvals
$testEnvironmentData = @{
    name = "test"
    wait_timer = 0
    reviewers = @(
        @{
            type = "User"
            id = 123456 # Replace with actual user ID
        }
    )
    deployment_branch_policy = @{
        protected_branches = $false
        custom_branch_policies = $true
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/environments/test" `
    -Method PUT `
    -Headers @{
        Authorization = "token $GitHubToken"
        "Content-Type" = "application/json"
        Accept = "application/vnd.github.v3+json"
    } `
    -Body $testEnvironmentData

# Create Production Environment with approvals
$prodEnvironmentData = @{
    name = "production"
    wait_timer = 600 # 10 minute wait
    reviewers = @(
        @{
            type = "User"
            id = 123456 # Replace with actual user ID
        }
    )
    deployment_branch_policy = @{
        protected_branches = $true
        custom_branch_policies = $false
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/environments/production" `
    -Method PUT `
    -Headers @{
        Authorization = "token $GitHubToken"
        "Content-Type" = "application/json"
        Accept = "application/vnd.github.v3+json"
    } `
    -Body $prodEnvironmentData

# 3. Create GitHub Secrets
Write-Host "Creating GitHub Secrets..."

# Create AZURE_CREDENTIALS secret
$azureCredentialsSecretData = @{
    encrypted_value = ConvertTo-Base64($servicePrincipal)
    key_id = "012345678901234567"  # Replace with actual key ID from GitHub
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/actions/secrets/AZURE_CREDENTIALS" `
    -Method PUT `
    -Headers @{
        Authorization = "token $GitHubToken"
        "Content-Type" = "application/json"
        Accept = "application/vnd.github.v3+json"
    } `
    -Body $azureCredentialsSecretData

# Create Resource Group secrets
$secretsToCreate = @{
    "RESOURCE_GROUP_DEV" = $DevResourceGroupName
    "RESOURCE_GROUP_TEST" = $TestResourceGroupName
    "RESOURCE_GROUP_PROD" = $ProdResourceGroupName
}

foreach ($secretName in $secretsToCreate.Keys) {
    $secretValue = $secretsToCreate[$secretName]
    
    $secretData = @{
        encrypted_value = ConvertTo-Base64($secretValue)
        key_id = "012345678901234567"  # Replace with actual key ID from GitHub
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/actions/secrets/$secretName" `
        -Method PUT `
        -Headers @{
            Authorization = "token $GitHubToken"
            "Content-Type" = "application/json"
            Accept = "application/vnd.github.v3+json"
        } `
        -Body $secretData
}

# 4. Get Web App Names from Azure and create secrets
Write-Host "Getting Web App names from Azure and creating secrets..."

$devWebAppName = az webapp list --resource-group $DevResourceGroupName --query "[0].name" -o tsv
$testWebAppName = az webapp list --resource-group $TestResourceGroupName --query "[0].name" -o tsv
$prodWebAppName = az webapp list --resource-group $ProdResourceGroupName --query "[0].name" -o tsv

$webAppSecrets = @{
    "WEBAPP_NAME_DEV" = $devWebAppName
    "WEBAPP_NAME_TEST" = $testWebAppName
    "WEBAPP_NAME_PROD" = $prodWebAppName
}

foreach ($secretName in $webAppSecrets.Keys) {
    $secretValue = $webAppSecrets[$secretName]
    
    if ($secretValue) {
        $secretData = @{
            encrypted_value = ConvertTo-Base64($secretValue)
            key_id = "012345678901234567"  # Replace with actual key ID from GitHub
        } | ConvertTo-Json
        
        Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/actions/secrets/$secretName" `
            -Method PUT `
            -Headers @{
                Authorization = "token $GitHubToken"
                "Content-Type" = "application/json"
                Accept = "application/vnd.github.v3+json"
            } `
            -Body $secretData
    }
}

# 5. Configure Branch Protection Rules
Write-Host "Setting up branch protection rules..."

$branchProtectionRuleData = @{
    required_status_checks = @{
        strict = $true
        contexts = @(
            "build_and_test",
            "validate_bicep"
        )
    }
    enforce_admins = $true
    required_pull_request_reviews = @{
        dismissal_restrictions = @{}
        dismiss_stale_reviews = $true
        require_code_owner_reviews = $true
        required_approving_review_count = 1
    }
    restrictions = $null
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubRepoOwner/$GitHubRepoName/branches/main/protection" `
    -Method PUT `
    -Headers @{
        Authorization = "token $GitHubToken"
        "Content-Type" = "application/json"
        Accept = "application/vnd.github.luke-cage-preview+json"
    } `
    -Body $branchProtectionRuleData

Write-Host "GitHub repository configuration completed successfully!"
Write-Host "Next steps:"
Write-Host "1. Ensure you have Integration Tests project at src/BookShop.IntegrationTests"
Write-Host "2. Check that health endpoints are configured correctly"
Write-Host "3. Configure deployment slots in production environment"
Write-Host "4. Set up CodeQL for security scanning"
