# CRM - Customer Relationship Management

A full-stack CRM application built with ASP.NET Core and Angular.

## Features

- **Customer Management**: Add, edit, delete, and search customers
- **Deal Management**: Sales pipeline with Kanban board view
- **Task Management**: Create and track tasks linked to customers/deals
- **Dashboard & Reports**: Key metrics, charts, and analytics
- **Authentication**: JWT-based auth with role-based access control

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core 8 Web API |
| Frontend | Angular 17 |
| Database | Microsoft SQL Server |
| Authentication | JWT + Refresh Tokens |
| ORM | Entity Framework Core |

## Project Structure

```
CRM/
├── plan/                    # Documentation and plans
├── backend/                 # .NET Backend
│   ├── CRM.API/            # Web API
│   ├── CRM.Core/           # Domain
│   ├── CRM.Application/    # Business Logic
│   ├── CRM.Infrastructure/ # Data Access
│   └── CRM.Tests/          # Tests
└── frontend/               # Angular Frontend
    └── crm-app/
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server)
- [Angular CLI](https://angular.io/cli)

### Backend Setup

```bash
cd backend

# Restore packages
dotnet restore

# Update database
dotnet ef database update --project CRM.Infrastructure --startup-project CRM.API

# Run API
dotnet run --project CRM.API
```

The API will be available at `https://localhost:7001`

### Frontend Setup

```bash
cd frontend/crm-app

# Install dependencies
npm install

# Run development server
ng serve
```

The app will be available at `http://localhost:4200`

## API Documentation

Once the backend is running, access Swagger UI at:
`https://localhost:7001/swagger`

## User Roles

| Role | Permissions |
|------|-------------|
| Admin | Full access to all features |
| SalesManager | Manage team, view reports |
| SalesRep | Manage own customers, deals, tasks |

## License

MIT License
