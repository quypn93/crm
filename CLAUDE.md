# CRM Project - Claude Instructions

## Project Overview
This is a CRM (Customer Relationship Management) web application built with:
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: Angular 17
- **Database**: Microsoft SQL Server

## Project Structure

```
d:\Task\CRM\
├── plan/                    # Implementation plans and documentation
├── backend/                 # .NET Backend API
│   ├── CRM.API/            # Web API project (Controllers, Middlewares)
│   ├── CRM.Core/           # Domain layer (Entities, Interfaces)
│   ├── CRM.Application/    # Application layer (Services, DTOs)
│   ├── CRM.Infrastructure/ # Infrastructure layer (Data, Repositories)
│   └── CRM.Tests/          # Unit and Integration tests
└── frontend/               # Angular Frontend
    └── crm-app/            # Angular application
```

## Development Commands

### Backend (.NET)
```bash
# Navigate to backend
cd backend

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project CRM.API

# Run tests
dotnet test

# Add migration
dotnet ef migrations add <MigrationName> --project CRM.Infrastructure --startup-project CRM.API

# Update database
dotnet ef database update --project CRM.Infrastructure --startup-project CRM.API
```

### Frontend (Angular)
```bash
# Navigate to frontend
cd frontend/crm-app

# Install dependencies
npm install

# Run development server
ng serve

# Build for production
ng build --configuration production

# Run tests
ng test

# Generate component
ng generate component features/<feature-name>/components/<component-name>
```

## Architecture Guidelines

### Backend (Clean Architecture)
1. **CRM.Core**: Contains entities, interfaces, enums. No external dependencies.
2. **CRM.Application**: Contains business logic, services, DTOs, validators. Depends on Core.
3. **CRM.Infrastructure**: Contains EF Core, repositories, external services. Depends on Core & Application.
4. **CRM.API**: Entry point, controllers, middlewares. Depends on all layers.

### Frontend (Feature-based)
1. **core/**: Singleton services, guards, interceptors (imported once in AppModule)
2. **shared/**: Reusable components, directives, pipes (imported in feature modules)
3. **features/**: Lazy-loaded feature modules (auth, dashboard, customers, deals, tasks, reports)
4. **layout/**: Main layout components (header, sidebar, footer)

## Coding Standards

### C# (.NET)
- Use async/await for all I/O operations
- Follow SOLID principles
- Use DTOs for API requests/responses (never expose entities directly)
- Validate input with FluentValidation
- Use AutoMapper for object mapping
- Handle exceptions with global middleware

### TypeScript (Angular)
- Use strict mode
- Prefer reactive forms over template-driven forms
- Use services for API calls and state management
- Implement lazy loading for feature modules
- Use OnPush change detection where possible

## Key Files

### Backend
- `CRM.API/Program.cs` - Application entry point and DI configuration
- `CRM.Infrastructure/Data/CrmDbContext.cs` - EF Core database context
- `CRM.Core/Entities/` - Domain entities
- `CRM.Application/Services/` - Business logic services

### Frontend
- `src/app/app.module.ts` - Root module
- `src/app/app-routing.module.ts` - Main routing configuration
- `src/app/core/services/auth.service.ts` - Authentication service
- `src/app/core/interceptors/auth.interceptor.ts` - JWT token interceptor

## Database

### Connection String (Development)
```
Server=localhost;Database=CRM_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

### Main Tables
- Users, Roles, UserRoles
- Customers
- DealStages, Deals
- Tasks
- ActivityLogs

## API Endpoints

Base URL: `https://localhost:7001/api`

- `/auth` - Authentication (login, register, refresh-token)
- `/users` - User management
- `/customers` - Customer CRUD
- `/deals` - Deal/Transaction management
- `/tasks` - Task management
- `/dashboard` - Dashboard data
- `/reports` - Reports and analytics

## Environment Variables

### Backend (appsettings.json)
- `ConnectionStrings:DefaultConnection` - SQL Server connection
- `JwtSettings:Secret` - JWT signing key
- `JwtSettings:Issuer` - Token issuer
- `JwtSettings:Audience` - Token audience

### Frontend (environment.ts)
- `apiUrl` - Backend API URL
- `production` - Production flag
