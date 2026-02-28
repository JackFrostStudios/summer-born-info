---
paths:
  - "API/**"
---
# Development Flow Guide

## Overview
This document outlines the recommended development workflow for the Summer Born Information project. It emphasizes **Test-Driven Development (TDD)** and the **Red-Green-Refactor** cycle to ensure high-quality, maintainable code.

## Core Principles

### 1. Test-Driven Development (TDD)
Write tests **before** writing implementation code. This approach:
- Ensures code is testable by design
- Clarifies requirements before implementation
- Provides a safety net for refactoring
- Results in better code coverage and quality

### 2. Red-Green-Refactor Cycle
Follow this iterative cycle for every change:

```
RED   → Write a failing test that defines the desired behavior
GREEN → Write minimal code to make the test pass
REFACTOR → Improve code quality while keeping tests passing
```

### 3. Continuous Verification
After **every** change:
```bash
dotnet build
dotnet test
```

## Development Workflow

### Step 1: Red - Write a Failing Test

1. **Identify the requirement**: What functionality needs to be implemented?
2. **Write a test** that describes the expected behavior
3. **Run the test** to confirm it fails (RED phase)
4. **Ensure the failure is meaningful** - the test should fail for the right reason

### Step 2: Green - Make the Test Pass

Write the **minimum** code necessary to make the test pass.

1. **Create the command/query class** if it doesn't exist
2. **Implement the handler** with basic logic
3. **Add necessary infrastructure** (DbContext, repositories, etc.)
4. **Run the test** to confirm it passes (GREEN phase)

### Step 3: Refactor - Improve Code Quality

With tests passing, improve the code:

1. **Eliminate duplication**
2. **Extract methods/classes** for better organization
3. **Improve naming** for clarity
4. **Apply design patterns** where appropriate
5. **Optimize performance** if needed
6. **Add error handling** and edge cases

**Important**: Run tests after each refactoring to ensure nothing breaks.

## Testing Strategy

### Unit Tests
Test individual components in isolation.

**Where to place tests:**
- `Domain.Tests/` - Domain entities and value objects
- `Features.Tests/` - Command/Query handlers and validators
- `Infrastructure.Tests/` - Repository implementations

**Mocking:**
- Use Moq or NSubstitute for dependencies
- Mock DbContext for handler tests
- Use in-memory database for repository tests

**Example Test Structure:**
```csharp
public class CreateSchoolCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesSchool()
    {
        // Arrange
        var mockContext = new Mock<ApplicationDbContext>();
        var handler = new CreateSchoolCommandHandler(mockContext.Object);
        var command = new CreateSchoolCommand { Name = "Test", LocalAuthority = "LA" };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_NullCommand_ThrowsException()
    {
        // Arrange
        var mockContext = new Mock<ApplicationDbContext>();
        var handler = new CreateSchoolCommandHandler(mockContext.Object);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.Handle(null, CancellationToken.None));
    }
}
```

### Integration Tests
Test complete workflows with real dependencies.

**Where to place tests:** `Web.IntegrationTests/`

**Setup:**
- Use TestContainers to spin up PostgreSQL
- Use `CustomWebApplicationFactory` for test server
- Seed test data before each test

**Example:**
```csharp
public class SchoolsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public SchoolsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Post_School_Returns201()
    {
        // Arrange
        var content = new StringContent(
            JsonSerializer.Serialize(new { name = "Test", localAuthority = "LA" }),
            Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/schools", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## Build and Test Commands

### Essential Commands

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test API/Features.Tests/
```