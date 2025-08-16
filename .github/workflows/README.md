# CI/CD Setup Guide for BookShop Application

This guide provides instructions for setting up Continuous Integration and Continuous Deployment pipelines for the BookShop application.

## Prerequisites

- GitHub repository for the BookShop application
- Azure subscription with necessary permissions
- GitHub Personal Access Token with repo and workflow scopes
- Azure CLI installed and configured

## Getting Started

1. **Run the configuration script** to set up GitHub repository settings and Azure Service Principal:

```powershell
cd scripts
./configure-github-cicd.ps1 `
    -GitHubRepoOwner "YourUsername" `
    -GitHubRepoName "multitierapp" `
    -GitHubToken "your-github-token" `
    -SubscriptionId "your-subscription-id" `
    -DevResourceGroupName "bookshop-dev-rg" `
    -TestResourceGroupName "bookshop-test-rg" `
    -ProdResourceGroupName "bookshop-prod-rg"
```

2. **Create deployment slots** for the production App Service:

```powershell
az webapp deployment slot create --name bookshop-app-prod --resource-group bookshop-prod-rg --slot staging
```

3. **Set up monitoring alerts** in Azure:

```powershell
# Create an action group for notifications
az monitor action-group create --name BookshopAlerts --resource-group bookshop-prod-rg --short-name BookshopAlerts --email-receiver admin admin@example.com

# Create alert for high server response time
az monitor metrics alert create --name HighResponseTime --resource-group bookshop-prod-rg --scopes "/subscriptions/{subscriptionId}/resourceGroups/bookshop-prod-rg/providers/Microsoft.Web/sites/bookshop-app-prod" --condition "avg Response Time > 5000 millisecond" --window-size 5m --evaluation-frequency 1m --action "/subscriptions/{subscriptionId}/resourceGroups/bookshop-prod-rg/providers/Microsoft.Insights/actionGroups/BookshopAlerts"
```

4. **Create WebHook for deployment notifications** (optional):

```powershell
az webapp deployment github-actions add --resource-group bookshop-prod-rg --name bookshop-app-prod --repo YourUsername/multitierapp --token "your-github-token" --runtime "dotnet:8" --branch main
```

## CI/CD Pipeline Structure

The CI/CD setup includes three GitHub Actions workflows:

- **ci.yml** - Continuous Integration
- **cd.yml** - Continuous Deployment
- **security-scan.yml** - Security Scanning

For detailed information about the workflow structure and functionality, see [CICD-Pipeline.md](../docs/CICD-Pipeline.md).

## Environment Configuration

### Development Environment
- Automatic deployments from main branch
- No approval required
- Resources in `bookshop-dev-rg` resource group

### Test Environment
- Manual triggering or promotion from development
- Requires approval
- Resources in `bookshop-test-rg` resource group

### Production Environment
- Manual triggering or promotion from test
- Requires approval
- Includes deployment slots for blue-green deployment
- Resources in `bookshop-prod-rg` resource group

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure DevOps Documentation](https://docs.microsoft.com/en-us/azure/devops)
- [Azure App Service Deployment](https://docs.microsoft.com/en-us/azure/app-service/deploy-github-actions)
