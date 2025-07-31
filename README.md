# TeslaStarter

A starting point for integrating your App with Tesla and more!

## Project Structure

```
tesla-starter/
├── src/
│   ├── apps/
│   │   ├── api/              # ASP.NET Core Web API
│   │   └── web/              # React TypeScript Frontend
│   ├── services/             # .NET Business Logic
│   │   ├── TeslaStarter.Domain/
│   │   ├── TeslaStarter.Application/
│   │   └── TeslaStarter.Infrastructure/
│   └── shared/
│       ├── dotnet/           # Shared .NET libraries
│       └── typescript/       # Shared TypeScript types
├── tests/                    # All test projects
├── docs/                     # Documentation and ADRs
└── scripts/                  # Build and deployment scripts
```

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- pnpm
- Docker (for local development)
- PostgreSQL (or Docker)
- Descope account for authentication

## Getting Started

### Environment Setup

1. **Backend Configuration** - Create `src/apps/api/TeslaStarter.Api/appsettings.local.json`:
```json
{
  "ConnectionStrings": {
    "TeslaStarterDb": "Host=localhost;Database=teslastarter;Username=postgres;Password=postgres"
  },
  "Descope": {
    "ProjectId": "YOUR_DESCOPE_PROJECT_ID",
    "ManagementKey": "YOUR_DESCOPE_MANAGEMENT_KEY"
  },
  "Tesla": {
    "ClientId": "YOUR_TESLA_CLIENT_ID",
    "RedirectUri": "http://localhost:3000/tesla/callback"
  }
}
```

2. **Frontend Configuration** - Create `src/apps/web/.env`:
```env
VITE_DESCOPE_PROJECT_ID=YOUR_DESCOPE_PROJECT_ID
VITE_API_BASE_URL=http://localhost:5000/api
```

### Database Setup

```bash
# Start PostgreSQL with Docker
docker run -d \
  --name teslastarter-db \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=teslastarter \
  -p 5432:5432 \
  postgres:16

# Run migrations
cd src/apps/api/TeslaStarter.Api
dotnet ef database update
```

### Install dependencies
```bash
pnpm install
```

### Build everything
```bash
dotnet build
nx build web
```

### Run the API
```bash
cd src/apps/api/TeslaStarter.Api
dotnet run
# API will be available at https://localhost:5001 or http://localhost:5000
```

### Run the web app
```bash
cd src/apps/web
pnpm dev
# Web app will be available at http://localhost:3000
```

### Run tests
```bash
# .NET tests
dotnet test

# Frontend tests
nx test web
```

## Development

This is a monorepo using:
- **Nx** for JavaScript/TypeScript project management
- **.NET SDK** for C# projects
- **pnpm** for package management
- **Vite** for frontend tooling

## Architecture

See the Architecture Decision Records (ADRs) in the `docs/adr/` directory for detailed technical decisions.

## Features

### Authentication & Authorization
- **Descope Integration**: Secure authentication using Descope's hosted auth solution
- **JWT Bearer Tokens**: API authentication via Bearer tokens
- **User Synchronization**: Automatic user creation and profile updates on login
- **Protected Routes**: Frontend route protection with automatic redirects

### Tesla Integration
- **OAuth 2.0 Flow**: Secure Tesla account linking via OAuth
- **Token Management**: Automatic token refresh and secure storage
- **Account Linking**: Users can link/unlink Tesla accounts

### User Management
- **User Profiles**: Email, display name, and Tesla account status
- **User Directory**: View all registered users (authenticated endpoint)
- **Real-time Updates**: Profile changes reflected immediately

## API Endpoints

### Authentication
- `GET /api/auth/me` - Get current user profile
- `GET /api/auth/users` - List all users (requires auth)
- `GET /api/auth/tesla/authorize` - Initialize Tesla OAuth
- `POST /api/auth/tesla/callback` - Complete Tesla OAuth
- `POST /api/auth/tesla/refresh` - Refresh Tesla tokens
- `DELETE /api/auth/tesla/unlink` - Unlink Tesla account

### Users
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user profile

### Vehicles
- `GET /api/vehicles` - List user's vehicles
- `POST /api/vehicles` - Link a new vehicle
- `GET /api/vehicles/{id}` - Get vehicle details
- `PUT /api/vehicles/{id}` - Update vehicle details
- `DELETE /api/vehicles/{id}` - Unlink vehicle

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 9
- **Database**: PostgreSQL with Entity Framework Core
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure)
- **Authentication**: Descope + Custom JWT validation
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **State Management**: React Context + React Query
- **Routing**: React Router v7
- **UI Framework**: Tailwind CSS + ShadCN UI
- **Authentication**: Descope React SDK

## Security Considerations

- All sensitive configuration stored in environment variables
- Tesla tokens encrypted at rest
- HTTPS required in production
- CORS configured for specific origins
- Secure session management via Descope