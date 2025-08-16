# CI/CD Pipeline Documentation

## Overview

This document describes the CI/CD pipelines set up for the BookShop application. The pipelines are implemented using GitHub Actions and are designed to automate the build, test, and deployment processes across different environments.

## Pipeline Structure

The CI/CD setup consists of three main workflows:

1. **Continuous Integration (CI)** - Builds and tests the application on every push or pull request
2. **Continuous Deployment (CD)** - Deploys the application to different environments
3. **Security Scanning** - Performs regular security checks on the codebase

## Continuous Integration Workflow (`ci.yml`)

This workflow is triggered on:
- Push to main, feature/*, bugfix/*, and release/* branches
- Pull requests to main branch
- Manual trigger (workflow_dispatch)

### Jobs:

#### Build and Test
- Checkout the code
- Set up .NET 8
- Restore dependencies
- Build the solution
- Run tests with code coverage
- Generate code coverage report
- Run CodeQL security analysis

#### Validate Infrastructure
- Checkout the code
- Login to Azure
- Validate Bicep templates

## Continuous Deployment Workflow (`cd.yml`)

This workflow is triggered on:
- Push to the main branch (except for MD files and GitHub workflows)
- Manual trigger with environment selection

### Jobs:

#### Deploy to Development
- Checkout the code
- Login to Azure
- Set up .NET 8
- Build and publish the application
- Deploy infrastructure using Bicep templates
- Deploy the application to Azure App Service
- Run smoke tests

#### Deploy to Test (requires approval)
- Same steps as Dev deployment
- Adds integration tests

#### Deploy to Production (requires approval)
- Same steps as Dev deployment
- Deploys to staging slot first
- Runs tests on staging
- Swaps slots to production
- Verifies production deployment

## Security Scanning Workflow (`security-scan.yml`)

This workflow runs weekly and can be triggered manually.

### Jobs:

#### Security Scanning
- Checkout the code
- Run code style analysis
- Run OWASP dependency check
- Run CodeQL analysis
- Scan container images
- Scan Bicep templates for security issues

## Environment Configuration

The pipelines use GitHub Environments to manage deployments:

1. **Development Environment**
   - Automatic deployments
   - No approvals required

2. **Test Environment**
   - Requires successful deployment to development
   - Requires approval from QA team

3. **Production Environment**
   - Requires successful deployment to test
   - Requires approval from release managers
   - Uses blue-green deployment strategy with slots

## Required GitHub Secrets

The following secrets need to be configured in GitHub:

- `AZURE_CREDENTIALS` - Azure service principal credentials
- `RESOURCE_GROUP_DEV` - Name of the development resource group
- `RESOURCE_GROUP_TEST` - Name of the test resource group
- `RESOURCE_GROUP_PROD` - Name of the production resource group
- `WEBAPP_NAME_DEV` - Name of the development web app
- `WEBAPP_NAME_TEST` - Name of the test web app
- `WEBAPP_NAME_PROD` - Name of the production web app

## Quality Gates

The CI/CD pipelines include the following quality gates:

1. **Build Success** - The application must build successfully
2. **Test Coverage** - Code coverage must be above 80%
3. **Security Scan** - No critical security vulnerabilities
4. **Smoke Tests** - Basic functionality tests must pass
5. **Integration Tests** - More comprehensive tests must pass

## Monitoring and Alerts

The CI/CD pipeline integrates with Azure Application Insights to monitor:

1. **Deployment Success/Failure**
2. **Application Health**
3. **Performance Metrics**
4. **Error Rates**

Alerts are configured to notify the team of:

1. **Failed Deployments**
2. **High Error Rates**
3. **Performance Degradation**
4. **Security Vulnerabilities**

## Rollback Procedures

If a deployment fails or causes issues:

1. **Development/Test** - Fix the issue and redeploy
2. **Production** - Use the "Swap Slots" feature to roll back to the previous version

## Next Steps

1. Set up branch protection rules for the main branch
2. Configure required status checks
3. Set up automated dependency updates
4. Implement release notes generation
