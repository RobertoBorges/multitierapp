// BookShop Application Infrastructure
// Main Bicep template for Azure resources

targetScope = 'resourceGroup'

// Parameters
@description('The environment name (dev, test, prod)')
param environmentName string = 'dev'

@description('The Azure region for all resources')
param location string = resourceGroup().location

@description('The name of the application')
param applicationName string = 'bookshop'

@description('Tags for all resources')
param tags object = {
  application: applicationName
  environment: environmentName
}

// SQL Database parameters (for connection to existing database)
@description('Name of the existing SQL Server')
param sqlServerName string

@description('Name of the existing database')
param sqlDatabaseName string

// Variables
var appServicePlanName = '${applicationName}-plan-${environmentName}'
var appServiceName = '${applicationName}-app-${environmentName}'
var appInsightsName = '${applicationName}-insights-${environmentName}'
var logAnalyticsName = '${applicationName}-logs-${environmentName}'
var keyVaultName = '${applicationName}-kv-${environmentName}${uniqueString(resourceGroup().id)}'

// Resource modules
module logging 'modules/logging.bicep' = {
  name: 'loggingDeploy'
  params: {
    location: location
    tags: tags
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyVaultDeploy'
  params: {
    location: location
    tags: tags
    keyVaultName: keyVaultName
  }
}

module hosting 'modules/appservice.bicep' = {
  name: 'hostingDeploy'
  params: {
    location: location
    tags: tags
    appServicePlanName: appServicePlanName
    appServiceName: appServiceName
    appInsightsInstrumentationKey: logging.outputs.instrumentationKey
    environmentName: environmentName
  }
}

// SQL Connection Configuration - Use existing database
module sqlConnection 'modules/sqlconnection.bicep' = {
  name: 'sqlConnectionDeploy'
  params: {
    keyVaultName: keyVaultName
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
  }
  dependsOn: [
    keyVault
  ]
}

// Backup and Disaster Recovery Configuration
module backup 'modules/backup.bicep' = {
  name: 'backupDeploy'
  params: {
    location: location
    tags: tags
    appServiceName: appServiceName
    backupEnabled: true
  }
  dependsOn: [
    hosting
  ]
}

// Outputs
output appServiceUrl string = hosting.outputs.appServiceUrl
output appInsightsConnectionString string = logging.outputs.appInsightsConnectionString
output keyVaultUri string = keyVault.outputs.keyVaultUri
output sqlConnectionStringSecretUri string = sqlConnection.outputs.connectionStringSecretUri
output backupEnabled string = backup.outputs.backupEnabled ? 'Yes' : 'No'
output backupStorageAccountName string = backup.outputs.backupStorageAccountName
