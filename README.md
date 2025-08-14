# BookShop Multi-Tier Application (.NET Framework 3.5)

A legacy multi-tier book store application built on .NET Framework 3.5, demonstrating old-fashioned enterprise architecture patterns from the 2008 era. This application showcases a complete on-premises solution for a library bookstore with employee management and Windows Authentication.

## Architecture Overview

This application follows a traditional 3-tier architecture:

### 1. Presentation Layer
- **BookShop.Web** - Customer-facing website (ASP.NET Web Forms)
- **BookShop.Admin** - Employee administration panel with Windows Authentication

### 2. Business Logic Layer
- **BookShop.BusinessLogic** - Business rules, validation, and service orchestration

### 3. Data Access Layer
- **BookShop.DataAccess** - ADO.NET-based data access with SQL Server 2008
- **BookShop.Common** - Shared models and data transfer objects

### 4. Database
- SQL Server 2008 compatible database with complete schema and sample data

## Technologies Used

- .NET Framework 3.5
- ASP.NET Web Forms
- ADO.NET (DataSets/DataTables pattern)
- SQL Server 2008
- Windows Authentication
- IIS 7.0+
- Legacy CSS styling (2008 era)

## Prerequisites

- Windows Server 2008+ or Windows Vista/7/8/10
- IIS 7.0 or higher
- SQL Server 2008 or higher (Express edition supported)
- .NET Framework 3.5 Service Pack 1

## Database Setup

1. **Create Database:**
   ```sql
   -- Run the database creation script
   sqlcmd -S .\SQLEXPRESS -i "Database\01_CreateDatabase.sql"
   ```

2. **Populate Sample Data:**
   ```sql
   -- Run the data population script
   sqlcmd -S .\SQLEXPRESS -i "Database\02_PopulateData.sql"
   ```

3. **Update Connection Strings:**
   - Modify the connection string in both `BookShop.Web\web.config` and `BookShop.Admin\web.config`
   - Default: `Data Source=.\SQLEXPRESS;Initial Catalog=BookShopDB;Integrated Security=True`

## IIS Deployment

### Customer Website (BookShop.Web)
1. Create new IIS Application Pool:
   - Name: `BookShopWebPool`
   - .NET Framework: `v2.0.50727` (for .NET 3.5)
   - Managed Pipeline Mode: `Integrated`

2. Create IIS Website:
   - Site Name: `BookShop Customer Portal`
   - Port: `80` (or your preferred port)
   - Physical Path: `[YourPath]\BookShop.Web`
   - Application Pool: `BookShopWebPool`

### Admin Website (BookShop.Admin)
1. Create new IIS Application Pool:
   - Name: `BookShopAdminPool`
   - .NET Framework: `v2.0.50727` (for .NET 3.5)
   - Managed Pipeline Mode: `Integrated`
   - Identity: `ApplicationPoolIdentity` or domain service account

2. Create IIS Website:
   - Site Name: `BookShop Administration`
   - Port: `8080` (different from customer site)
   - Physical Path: `[YourPath]\BookShop.Admin`
   - Application Pool: `BookShopAdminPool`

3. **Configure Windows Authentication:**
   - Enable Windows Authentication in IIS for the admin site
   - Disable Anonymous Authentication
   - Add appropriate Windows users/groups to access the site

## Windows Authentication Setup

### Employee Accounts
The system expects Windows domain accounts in the format: `DOMAIN\username`

Sample employee accounts (update `Database\02_PopulateData.sql`):
- `BOOKSHOP\jsmith` (Admin)
- `BOOKSHOP\mjohnson` (Admin)
- `BOOKSHOP\rwilliams` (Employee)
- `BOOKSHOP\lbrown` (Employee)

### Admin Access
- Administrators have full access to all management functions
- Regular employees have restricted access to inventory and orders
- Access control is managed through the `Employees` table `IsAdmin` column

## Project Structure

```
BookShop.sln
├── BookShop.Common/               # Shared models and DTOs
│   ├── Models/                   # Domain models
│   └── Properties/               # Assembly info
├── BookShop.DataAccess/          # Data access layer
│   ├── Base/                     # Base repository class
│   ├── Repositories/             # Entity-specific repositories
│   └── Properties/               # Assembly info
├── BookShop.BusinessLogic/       # Business logic layer
│   ├── Services/                 # Business services
│   └── Properties/               # Assembly info
├── BookShop.Web/                 # Customer website
│   ├── css/                      # Stylesheets
│   ├── images/                   # Images and assets
│   ├── App_Code/                 # Code behind files
│   ├── bin/                      # Compiled assemblies
│   └── *.aspx                    # Web forms
├── BookShop.Admin/               # Admin website
│   ├── css/                      # Admin stylesheets
│   ├── Admin/                    # Admin-only pages
│   ├── App_Code/                 # Code behind files
│   └── *.aspx                    # Admin web forms
└── Database/                     # SQL scripts
    ├── 01_CreateDatabase.sql     # Database creation
    └── 02_PopulateData.sql       # Sample data
```

## Features

### Customer Features
- Browse books by category and author
- View book details and availability
- Legacy 2008-style user interface
- Responsive layout for the era

### Admin Features
- Windows Authentication for employees
- Dashboard with inventory statistics
- Book inventory management
- Order processing and tracking
- Employee management (admin only)
- Reporting capabilities
- Role-based access control

## Development Notes

This application intentionally uses legacy patterns and technologies to demonstrate:

- **ADO.NET DataSets/DataTables** instead of modern ORMs
- **ASP.NET Web Forms** instead of MVC
- **Master Pages** for layout consistency
- **Classic ASP.NET controls** (GridView, Repeater, etc.)
- **Inline CSS** and table-based layouts
- **Windows Authentication** with role-based security
- **Traditional 3-tier architecture** with clear separation

## Security Considerations

- Windows Authentication provides enterprise-grade security
- SQL injection protection through parameterized queries
- Role-based access control for admin functions
- Connection string encryption recommended for production

## Maintenance

- Database backups scheduled for midnight daily
- Application logs stored in Windows Event Log
- Performance monitoring through IIS logs
- Manual deployment process (typical for 2008 era)

## Support

For technical issues:
- Contact IT Department: ext. 1234
- Email: admin@bookshop.com
- System Administrator: John Smith

---

*This application represents a typical enterprise application from 2008, showcasing legacy patterns and technologies that were industry standard at the time.*