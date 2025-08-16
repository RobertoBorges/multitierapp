# PowerShell script to configure monitoring and alerts for the BookShop application in Azure
# This script sets up Application Insights alerts, Log Analytics queries, and email notifications

param (
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory = $true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory = $false)]
    [string]$EmailAddress = "",
    
    [Parameter(Mandatory = $false)]
    [string]$PhoneNumber = ""
)

# 1. Create Action Group for alerts
Write-Host "Creating action group for alerts..."
$actionGroupName = "$AppServiceName-alerts"
$shortName = "BookshopAlt"

$actionGroupParams = @()

if ($EmailAddress) {
    $actionGroupParams += "--email-receiver", "admin", $EmailAddress
}

if ($PhoneNumber) {
    $actionGroupParams += "--sms-receiver", "oncall", $PhoneNumber, "1"
}

az monitor action-group create --name $actionGroupName `
    --resource-group $ResourceGroupName `
    --short-name $shortName `
    @actionGroupParams

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create action group"
    exit 1
}

# Get action group ID for use in alerts
$actionGroupId = az monitor action-group show --name $actionGroupName --resource-group $ResourceGroupName --query id -o tsv

# 2. Set up Application Insights alerts
Write-Host "Setting up Application Insights alerts..."

# Get App Insights resource ID
$appInsightsName = az resource list --resource-group $ResourceGroupName --query "[?type=='microsoft.insights/components' && name contains '$AppServiceName'].name" -o tsv
$appInsightsId = az resource show --resource-group $ResourceGroupName --name $appInsightsName --resource-type "microsoft.insights/components" --query id -o tsv

# Create Server Response Time alert
Write-Host "Creating server response time alert..."
az monitor metrics alert create --name "$AppServiceName-high-response-time" `
    --resource-group $ResourceGroupName `
    --scopes $appInsightsId `
    --condition "avg serverResponseTime > 3 second" `
    --description "Alert when server response time exceeds 3 seconds" `
    --evaluation-frequency 5m `
    --window-size 5m `
    --severity 2 `
    --action $actionGroupId

# Create Failed Requests alert
Write-Host "Creating failed requests alert..."
az monitor metrics alert create --name "$AppServiceName-failed-requests" `
    --resource-group $ResourceGroupName `
    --scopes $appInsightsId `
    --condition "count requests/failed > 5" `
    --description "Alert when there are more than 5 failed requests within 5 minutes" `
    --evaluation-frequency 5m `
    --window-size 5m `
    --severity 1 `
    --action $actionGroupId

# Create Server Exceptions alert
Write-Host "Creating server exceptions alert..."
az monitor metrics alert create --name "$AppServiceName-exceptions" `
    --resource-group $ResourceGroupName `
    --scopes $appInsightsId `
    --condition "count exceptions > 0" `
    --description "Alert when exceptions occur" `
    --evaluation-frequency 5m `
    --window-size 5m `
    --severity 1 `
    --action $actionGroupId

# 3. Set up Log Analytics workspace queries
Write-Host "Setting up Log Analytics workspace queries..."

# Get Log Analytics workspace ID
$workspaceName = az resource list --resource-group $ResourceGroupName --query "[?type=='microsoft.operationalinsights/workspaces'].name" -o tsv
$workspaceId = az resource show --resource-group $ResourceGroupName --name $workspaceName --resource-type "microsoft.operationalinsights/workspaces" --query id -o tsv

# Create query for authentication failures
$authFailuresQuery = @"
AppEvents
| where TimeGenerated > ago(1h)
| where AppRoleName contains '$AppServiceName'
| where EventName contains 'Authentication' and EventName contains 'Failure'
| summarize count() by bin(TimeGenerated, 5m), EventName
"@

az monitor log-analytics saved-search create --resource-group $ResourceGroupName `
    --workspace-name $workspaceName `
    --name "AuthenticationFailures" `
    --display-name "Authentication Failures" `
    --category "Security" `
    --query $authFailuresQuery

# Create query for database connection issues
$dbConnectionQuery = @"
AppTraces
| where TimeGenerated > ago(1h)
| where AppRoleName contains '$AppServiceName'
| where Message contains 'database' or Message contains 'connection' or Message contains 'SQL'
| where SeverityLevel > 1
| summarize count() by bin(TimeGenerated, 5m), Message
"@

az monitor log-analytics saved-search create --resource-group $ResourceGroupName `
    --workspace-name $workspaceName `
    --name "DatabaseConnectionIssues" `
    --display-name "Database Connection Issues" `
    --category "Errors" `
    --query $dbConnectionQuery

# 4. Create scheduled query alerts
Write-Host "Creating scheduled query alerts..."

# Alert for repeated authentication failures
$authFailureAlertQuery = @"
AppEvents
| where TimeGenerated > ago(30m)
| where AppRoleName contains '$AppServiceName'
| where EventName contains 'Authentication' and EventName contains 'Failure'
| summarize FailureCount=count() by UserName=tostring(Properties.UserName), IPAddress=tostring(Properties.ClientIP)
| where FailureCount > 5
"@

az monitor scheduled-query create --name "$AppServiceName-auth-failures" `
    --resource-group $ResourceGroupName `
    --scopes $workspaceId `
    --description "Alert when multiple authentication failures occur for the same user" `
    --severity 1 `
    --evaluation-frequency 30m `
    --window-size 30m `
    --action-group $actionGroupId `
    --query "$authFailureAlertQuery" `
    --throttling 30

# 5. Create availability test
Write-Host "Creating availability test..."
az monitor app-insights web-test create --resource-group $ResourceGroupName `
    --app $appInsightsName `
    --name "$AppServiceName-availability" `
    --location eastus `
    --frequency 300 `
    --web-test-kind ping `
    --geo-locations "us-ca-sjc" "us-tx-sn1" "us-il-ch1" "us-va-ash" "us-fl-mia" `
    --enabled true `
    --retry-enabled true `
    --timeout 120 `
    --url "https://$AppServiceName.azurewebsites.net/health"

Write-Host "Monitoring and alerts setup completed successfully!"
Write-Host "You can view these alerts in the Azure Portal under:"
Write-Host "- Resource Group: $ResourceGroupName"
Write-Host "- Application Insights: $appInsightsName"
Write-Host "- Log Analytics workspace: $workspaceName"
Write-Host "- Action Group: $actionGroupName"
