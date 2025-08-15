# BookShop Application

BookShop is a modern ASP.NET Core MVC application for managing an online bookstore. This application is a migration from a legacy .NET Framework 3.5 application to .NET 8, optimized for deployment to Azure.

## Project Structure

The application follows the Clean Architecture pattern:

- **BookShop.Core**: Domain models and interfaces
- **BookShop.Application**: Business logic and application services
- **BookShop.Infrastructure**: Data access, external services, and infrastructure concerns
- **BookShop.Web**: ASP.NET Core MVC web application

## Key Features

- Browse and search for books by title, author, or category
- View detailed book information including descriptions and pricing
- Shopping cart functionality
- User authentication with Microsoft Entra ID
- Admin dashboard for managing books, authors, and categories
- Responsive design with Bootstrap 5
- Modern database access using Entity Framework Core

## Technical Details

- **Framework**: .NET 8
- **Web Framework**: ASP.NET Core MVC
- **ORM**: Entity Framework Core (database-first approach)
- **Authentication**: Microsoft.Identity.Web (Entra ID)
- **Database**: Azure SQL Database
- **Logging**: Application Insights
- **Infrastructure as Code**: Azure Bicep
- **UI Framework**: Bootstrap 5
- **Deployment**: Azure App Service

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Azure account (for deployment)

### Local Development

1. Clone the repository
2. Update the connection string in `appsettings.Development.json`
3. Run the application using `dotnet run` in the BookShop.Web project directory

### Azure Configuration

1. Create an Entra ID app registration
2. Update the AzureAd configuration in `appsettings.json`
3. Configure the Azure SQL Database connection
4. Deploy the application to Azure App Service

## Authentication

This application uses Microsoft Entra ID for authentication. To configure authentication:

1. Create an app registration in the Entra ID portal
2. Configure the callback URL to match your application URL
3. Update the AzureAd configuration in `appsettings.json`

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "Domain": "yourdomain.onmicrosoft.com",
  "TenantId": "your-tenant-id",
  "ClientId": "your-client-id",
  "CallbackPath": "/signin-oidc",
  "SignedOutCallbackPath": "/signout-callback-oidc"
}
```

## Database Configuration

The application uses Entity Framework Core with a database-first approach to connect to an Azure SQL Database:

```json
"ConnectionStrings": {
  "BookShopDB": "Server=your-azure-sql-server.database.windows.net;Database=BookShopDB;Authentication=Active Directory Default;"
}
```

## Deployment

The application is designed for deployment to Azure App Service with the following considerations:

- Uses managed identity for secure database access
- Implements health checks for application monitoring
- Integrates with Application Insights for telemetry
- Configures automatic database migrations on startup

## License

This project is licensed under the MIT License - see the LICENSE file for details.
