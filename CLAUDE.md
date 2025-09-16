# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**NxB.BookingApi** is a consolidated .NET 9.0 Web API that merges functionality from multiple separate NxB microservices into a single booking-focused service. This appears to be part of a larger NxB business management system that was originally distributed across multiple Service Fabric microservices.

## Architecture

### Consolidated API Structure
This API consolidates functionality from four main business domains:
- **Ordering** - Order processing and management (`Controllers/Ordering/`)
- **Accounting** - Financial operations and customer management (`Controllers/Accounting/`)
- **Inventory** - Inventory tracking and management (`Controllers/Inventory/`)
- **Login** - Authentication and user management (`Controllers/Login/`)

### Dependencies Structure
The project includes local dependencies in the `Dependencies/` folder:
- **Munk.AspNetCore** - Custom ASP.NET Core extensions
- **Munk.Azure** - Azure integration utilities
- **Munk.Utils.Object** - Object utilities
- **NxB.Domain.Common** - Common domain models and logic
- **NxB.Dto** - Data transfer objects
- **NxB.Allocating.Shared.Infrastructure** - Allocation system infrastructure
- **NxB.Allocating.Shared.Model** - Allocation domain models
- **NxB.Settings.Shared.Infrastructure** - Settings management

### Key Technologies
- **.NET 9.0** with ASP.NET Core Web API
- **Entity Framework Core 9.0** with SQL Server
- **AutoMapper 12.0** for object mapping
- **Application Insights** for telemetry and monitoring
- **ServiceStack** for service communication
- **QuickPay SDK** for payment processing
- **Docker** support with Linux containers

## Development Commands

### Building the Solution
```bash
# Build the main project
dotnet build NxB.BookingApi.sln

# Build in Release mode
dotnet build NxB.BookingApi.sln -c Release

# Restore packages
dotnet restore NxB.BookingApi.sln
```

### Running the Application
```bash
# Run the API locally
dotnet run --project NxB.BookingApi/NxB.BookingApi.csproj

# Run with specific environment
dotnet run --project NxB.BookingApi/NxB.BookingApi.csproj --environment Development
```

### Docker Operations
```bash
# Build Docker image
docker build -f NxB.BookingApi/Dockerfile .

# Run containerized service
docker run -p 8080:80 <image-name>
```

### Testing
No dedicated test projects were found in this repository. Testing should be implemented using standard .NET testing frameworks if needed.

## Configuration

### Connection Strings
- **DefaultConnection** - SQL Server database connection (configured in appsettings.json)
- **ApplicationInsights.ConnectionString** - Application Insights telemetry

### Service Configuration
The `Program.cs` includes configuration methods for each consolidated service area:
- `ConfigureOrderingServices()` - Ordering-specific services
- `ConfigureAccountingServices()` - Accounting-specific services
- `ConfigureInventoryServices()` - Inventory-specific services
- `ConfigureLoginServices()` - Authentication-specific services

## Database Architecture

The application uses Entity Framework Core with SQL Server. The `AppDbContext` includes entities for:
- User management (Users, UserSessions, UserTenantAccess)
- Additional domain entities from the consolidated services

## Development Notes

- **Target Framework**: .NET 9.0
- **Runtime**: Cross-platform (originally migrated from Service Fabric Windows-specific deployment)
- **Container**: Linux containers via Docker
- **Service Communication**: Appears to be transitioning from Service Fabric remoting to standard HTTP APIs
- **Authentication**: Integrated authentication and authorization pipeline
- **Monitoring**: Application Insights integration for telemetry

## Important Patterns

- Controllers are organized by business domain in separate folders
- Dependencies are included as local projects rather than NuGet packages
- Service configuration is modularized by business area
- AutoMapper is used extensively for object mapping
- Entity Framework with SQL Server for data persistence