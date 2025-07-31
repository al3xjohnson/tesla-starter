# ADR-001: Monorepo Architecture with C# API and React/TypeScript Frontend

## Date
2025-07-22

## Status
Proposed

## Context
The TeslaStarter project requires both a backend API to handle Tesla API integration and data processing, and a frontend application to provide the gamified user experience. We need to decide on the repository structure and technology stack.

## Decision
We will use a monorepo structure with:
- **Backend**: C# with ASP.NET Core Web API
- **Frontend**: React with TypeScript
- **Repository Structure**: Single repository containing both projects

## Consequences

### Benefits

1. **Unified Development Experience**
   - Single repository to clone and manage
   - Atomic commits across frontend and backend
   - Simplified CI/CD pipeline configuration
   - Easier code reviews spanning full-stack changes

2. **Shared Resources**
   - Common configuration files (prettier, eslint, gitignore)
   - Shared TypeScript types/interfaces between frontend and backend
   - Centralized documentation
   - Single issue tracker and project board

3. **Technology Advantages**
   - **C# ASP.NET Core**: Enterprise-grade performance, excellent Tesla API client libraries, robust async/await support for API calls, built-in dependency injection
   - **React/TypeScript**: Type safety across the frontend, excellent animation ecosystem (Framer Motion, Lottie), large community support, progressive web app capabilities

4. **Development Efficiency**
   - Hot reload in both frontend and backend
   - Simplified local development setup
   - Consistent tooling and scripts
   - Easy API contract sharing via OpenAPI/Swagger

5. **Deployment Flexibility**
   - Can deploy frontend and backend independently
   - Support for various hosting options (Azure, AWS, containerized)
   - Easy to implement API versioning

### Potential Drawbacks

1. **Repository Size**: May grow large over time (mitigated by Git LFS for assets)
2. **Build Complexity**: Need to manage multiple build systems (mitigated by npm scripts)
3. **Team Scaling**: Larger teams might prefer separate repos (not a concern for initial development)

## Proposed Structure

```
tesla-starter/
├── .github/                    # GitHub Actions CI/CD
├── docs/                       # Documentation
│   ├── adr/                   # Architecture Decision Records
│   └── prd/                   # Product Requirements
├── src/
│   ├── apps/                  # Application projects
│   │   ├── api/              # ASP.NET Core Web API
│   │   │   └── TeslaStarter.Api/
│   │   └── web/              # React TypeScript Frontend
│   │       ├── src/
│   │       ├── public/
│   │       ├── package.json
│   │       └── tsconfig.json
│   ├── services/              # Business logic and infrastructure
│   │   ├── TeslaStarter.Domain/         # Domain models and interfaces
│   │   ├── TeslaStarter.Core/           # Business logic and use cases
│   │   ├── TeslaStarter.Infrastructure/ # External services, Tesla API
│   │   └── Common.Domain/              # Shared domain building blocks
│   └── shared/                # Shared code and contracts
│       └── typescript/        # Shared TypeScript types/interfaces
│           ├── api-contracts/
│           └── models/
├── tests/                     # All test projects
│   ├── api/                  # API integration and unit tests
│   │   └── TeslaStarter.Api.Tests/
│   └── services/             # Service layer tests
│       ├── TeslaStarter.Domain.Tests/
│       ├── TeslaStarter.Core.Tests/
│       └── TeslaStarter.Infrastructure.Tests/
├── scripts/                   # Build and deployment scripts
├── .gitignore
├── README.md
├── docker-compose.yml         # Local development environment
└── TeslaStarter.sln           # Solution file for all .NET projects
```

### Structure Benefits

1. **Clear Separation of Concerns**
   - `apps/` contains deployable applications
   - `services/` contains reusable business logic
   - `shared/` contains code shared across projects
   - `tests/` separates test projects from implementation

2. **Domain-Driven Design Support**
   - `Domain` project for core business entities
   - `Core` project for application business logic
   - `Infrastructure` project for external integrations
   - Clear boundaries between layers

3. **Test Organization**
   - Tests separated by layer (api, services)
   - Each project has corresponding test project
   - Supports unit, integration, and E2E testing
   - Test isolation from production code

4. **Scalability**
   - Easy to add new apps (mobile, admin panel)
   - Services can be extracted to microservices if needed
   - Shared code prevents duplication
   - Test projects scale with implementation

5. **Type Safety Across Stack**
   - TypeScript contracts generated from C# models
   - Shared validation rules
   - Consistent API contracts

## Implementation Plan

1. Create base monorepo structure with apps/, services/, and shared/ directories
2. Initialize .NET solution and projects:
   - Create TeslaStarter.Domain in services/ for domain models
   - Create TeslaStarter.Core in services/ for business logic
   - Create TeslaStarter.Infrastructure in services/ for Tesla API integration
   - Create TeslaStarter.Api in apps/api/ for Web API endpoints
   - Create Common.Domain in shared/dotnet/ for domain building blocks
3. Initialize React project with TypeScript and Vite in apps/web/
4. Set up shared TypeScript definitions in shared/typescript/
5. Configure development scripts for running both projects
6. Set up Docker Compose for local development
7. Configure CI/CD pipelines for independent deployments

## References
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Create React App with TypeScript](https://create-react-app.dev/docs/adding-typescript/)
- [Monorepo Best Practices](https://monorepo.tools/)