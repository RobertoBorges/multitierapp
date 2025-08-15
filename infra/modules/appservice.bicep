// App Service Module
// Provisions App Service Plan and App Service for BookShop application

param location string
param tags object
param appServicePlanName string
param appServiceName string
param appInsightsInstrumentationKey string
param environmentName string

// App Service Plan (the server farm that hosts the app)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: environmentName == 'prod' ? 'P1V3' : 'S1'
    tier: environmentName == 'prod' ? 'PremiumV3' : 'Standard'
    size: environmentName == 'prod' ? 'P1V3' : 'S1'
    family: environmentName == 'prod' ? 'Pv3' : 'S'
    capacity: 1
  }
  kind: 'windows'
  properties: {
    reserved: false // false for Windows, true for Linux
  }
}

// App Service (the web app)
resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceName
  location: location
  tags: tags
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      webSocketsEnabled: true
      alwaysOn: true
      use32BitWorkerProcess: false
      healthCheckPath: '/health'
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
      }
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsightsInstrumentationKey}'
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName == 'prod' ? 'Production' : environmentName == 'test' ? 'Staging' : 'Development'
        }
      ]
    }
  }
}

output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
