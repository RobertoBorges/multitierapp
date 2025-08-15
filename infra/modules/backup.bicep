// Backup and Disaster Recovery Module
// Configures backup and recovery options for the BookShop application

param location string
param tags object
param appServiceName string
param backupEnabled bool = true
param backupSchedule object = {
  frequencyInterval: 1
  frequencyUnit: 'Day'
  keepAtLeastOneBackup: true
  retentionPeriodInDays: 30
  startTime: '2022-01-01T01:00:00Z' // Will be updated to next available time
}

// Reference to the App Service
resource appService 'Microsoft.Web/sites@2022-03-01' existing = {
  name: appServiceName
}

// Configure Backup settings for App Service
resource backupConfig 'Microsoft.Web/sites/config@2022-03-01' = if (backupEnabled) {
  parent: appService
  name: 'backup'
  properties: {
    backupSchedule: backupSchedule
    enabled: true
    storageAccountUrl: backupEnabled ? 'https://${backupStorage.name}.blob.${environment().suffixes.storage}/${backupContainerName}${sasToken}' : ''
  }
}

// Variables
var backupContainerName = 'appbackups'
var sasToken = '?sv=2021-10-04&ss=b&srt=sco&sp=rwdlaciytfx&se=2099-12-31T23:59:59Z'  // Note: In production, generate this properly

// Create a backup storage account if needed
resource backupStorage 'Microsoft.Storage/storageAccounts@2022-05-01' = if (backupEnabled) {
  name: 'backup${appServiceName}${uniqueString(resourceGroup().id)}'
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

// Output
output backupEnabled bool = backupEnabled
output backupStorageAccountName string = backupEnabled ? backupStorage.name : ''
