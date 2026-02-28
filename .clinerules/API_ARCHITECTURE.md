---
paths:
  - "API/**"
---
# Architecture Documentation

## Overview

This project follows a **Clean Architecture** pattern with a **Feature-based organization** approach. The architecture is designed to ensure separation of concerns, testability, and maintainability.

## Architectural Layers

### 1. Domain Layer (`API/Domain/`)
The innermost layer containing business entities and domain logic.

**Responsibilities:**
- Define core business entities (e.g., `School`)
- Implement domain services and value objects
- Maintain domain invariants and business rules

**Key Components:**
- `Entities/`: Business entities with properties and domain methods
- No dependencies on external frameworks or other layers

### 2. Features Layer (`API/Features/`)
Implements use cases and application logic using the MediatR pattern (CQRS).

**Responsibilities:**
- Define commands and queries (CQRS pattern)
- Implement request handlers for each use case
- Coordinate between Domain and Infrastructure layers
- Validate input.
- Register Minimal API endpoints

**Structure:**
Each feature is organized in its own directory:

**Pattern:**
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, List)
- **Handlers**: Implement a ExecuteAsync function to process a request.
- **Endpoints**: Contains an extension method to register a minimal API endpoint.

### 3. Infrastructure Layer (`API/Infrastructure/`)
Handles data persistence and external dependencies.

**Responsibilities:**
- Implement repository patterns
- Configure Entity Framework Core DbContext
- Handle database migrations and connections
- Integrate with external services (email, storage, etc.)

**Key Components:**
- `Persistence/`: DbContext and entity configurations
- `Repositories/`: Repository implementations
- `Services/`: External service integrations

### 4. Web Layer (`API/Web/`)
Presentation layer with API controllers and application entry point.

**Responsibilities:**
- Configure services and middleware (Program.cs)
- Define API endpoints using Minimal APIs
- Handle HTTP requests and responses
- Configure authentication, authorization, and CORS
- Environment-specific configuration

**Key Files:**
- `Program.cs`: Application startup and endpoint configuration
- `appsettings.json`: Configuration settings
- `Properties/launchSettings.json`: Development environment settings

## Technology Stack

### Core Framework
- **.NET 10.0**: Modern C# features and performance improvements
- **ASP.NET Core**: Web API framework

### Architecture & Patterns
- **Entity Framework Core**: ORM for data access
- **xUnit**: Unit testing framework

### Database
- **PostgreSQL**: Primary database
- **TestContainers**: Integration testing with real database

### Development Tools
- **Docker**: Containerization for development and testing
- **DevContainers**: Consistent development environment

## Dependency Flow

```
Web → Features → Domain
      ↓         ↑
Infrastructure →
```

**Rule**: Dependencies can only point inward. Outer layers can depend on inner layers, but inner layers must never depend on outer layers.

## Testing Strategy

### Unit Tests
- **Domain.Tests**: Test domain entities and business logic
- **Features.Tests**: Test command/query handlers and validators
- **Infrastructure.Tests**: Test repository implementations

### Integration Tests
- **Web.IntegrationTests**: Test API endpoints with real database
- Use TestContainers to spin up PostgreSQL instance
- Test complete request/response cycles

## Configuration Management

- **Central Package Management**: `Directory.Packages.props` manages all NuGet package versions
- **Environment-specific settings**: `appsettings.Development.json` for local development
- **User secrets**: For sensitive data in development
- **Environment variables**: For production configuration