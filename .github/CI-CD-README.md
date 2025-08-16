# CI/CD Workflow Documentation

## Overview

This repository uses GitHub Actions for Continuous Integration (CI) to build and validate the BookShop multi-tier .NET Framework 3.5 application.

## Workflow: `ci.yml`

The CI workflow is triggered on:
- Push to `main` and `develop` branches
- Pull requests to `main` branch

### Jobs

#### 1. Build Job (`build`)
- **Runner**: Windows 2019 (optimal for .NET Framework 3.5)
- **Responsibilities**:
  - Checkout source code
  - Setup MSBuild with Visual Studio 2019 tools
  - Setup NuGet package manager
  - Restore NuGet packages
  - Build the complete solution
  - Build individual class libraries
  - Compile ASP.NET websites using `aspnet_compiler`
  - Upload build artifacts and database scripts

#### 2. Code Quality Job (`code-quality`)
- **Runner**: Windows 2019
- **Dependencies**: Requires successful `build` job
- **Responsibilities**:
  - Basic code analysis for legacy codebase
  - File structure validation
  - Search for TODO/FIXME/HACK comments

#### 3. Deployment Preparation Job (`prepare-deployment`)
- **Runner**: Windows 2019
- **Dependencies**: Requires successful `build` job
- **Trigger**: Only runs on `main` branch
- **Responsibilities**:
  - Download build artifacts
  - Create comprehensive deployment package
  - Include documentation (IIS deployment guide, README)
  - Upload deployment package

## Artifacts

The workflow produces the following artifacts:

1. **bookshop-build-artifacts**: Contains compiled DLLs and precompiled websites
2. **database-scripts**: Database setup and migration scripts
3. **bookshop-deployment-package**: Complete deployment package (main branch only)

## .NET Framework 3.5 Considerations

- Uses Windows 2019 runner for proper .NET Framework 3.5 support
- MSBuild with Visual Studio 2019 tools for compatibility
- NuGet 5.x for package management
- ASP.NET precompilation using `aspnet_compiler`

## Local Development

To build locally, ensure you have:
- Visual Studio 2019 or MSBuild tools
- .NET Framework 3.5 SDK
- NuGet CLI tools

Build commands:
```cmd
nuget restore BookShop.sln
msbuild BookShop.sln /p:Configuration=Release /p:Platform="Any CPU"
```

## Deployment

Deployment artifacts include everything needed for IIS deployment. Refer to `IIS-Deployment-Guide.md` for detailed deployment instructions.