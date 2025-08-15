// SQL Database Connection Module
// This module configures the connection to an existing Azure SQL Database

param keyVaultName string
param sqlServerName string
param sqlDatabaseName string
param connectionStringSecretName string = 'SqlConnectionString'

// Reference the Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

// Define a secret for the SQL connection string
// This will be populated manually after deployment with the actual connection string
resource sqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: connectionStringSecretName
  properties: {
    contentType: 'text/plain'
    value: 'Server=tcp:${sqlServerName}.${environment().suffixes.sqlServerHostname},1433;Database=${sqlDatabaseName};Authentication=Active Directory Managed Identity;'
  }
}

// Output
output connectionStringSecretUri string = sqlConnectionStringSecret.properties.secretUri
