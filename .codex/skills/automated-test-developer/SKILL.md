---
name: automated-test-developer
description: Design and implement automated tests for this project, favoring integration tests and the existing xUnit/Testcontainers patterns.
---

# Automated Test Developer

Use this skill for test design and implementation.

## Rules

- Prefer integration tests when the behavior touches EF Core, HTTP endpoints, events, or file/large-object storage.
- Mock only dependencies that are truly external or unavailable in the test environment.
- Reuse the existing test framework before adding new helpers.
- Keep tests close to the feature they cover.
- Use xUnit and the project's current `Given_When_Then` naming style.
- When writing tests, analyze the inputs, the expected outputs, and the expected post-action system state.
- When writing tests, analyze edge cases and error scenarios as part of the test design.
- Assert on outputs and system state, not on whether a function was called.
- Prefer black-box tests: verify that the input leads to the desired output or system state rather than checking internal implementation details.

## Existing patterns

- Web integration tests inherit from [`API/SummerBornInfo.Web.IntegrationTests/TestFramework/WebIntegrationTestBase.cs`](../../../API/SummerBornInfo.Web.IntegrationTests/TestFramework/WebIntegrationTestBase.cs).
- Feature and infrastructure tests inherit from [`API/SummerBornInfo.IntegrationTests/IntegrationTestBase.cs`](../../../API/SummerBornInfo.IntegrationTests/IntegrationTestBase.cs).
- Database setup uses `IntegrationTestDatabaseServerFixture` and `IntegrationTestDatabaseInstanceFixture`.
- HTTP tests use `CustomWebApplicationFactory`.
- Event assertions live in [`API/SummerBornInfo.IntegrationTests/Assertions/EventAssertions.cs`](../../../API/SummerBornInfo.IntegrationTests/Assertions/EventAssertions.cs).
- Large object assertions live in [`API/SummerBornInfo.IntegrationTests/Assertions/LargeObjectAssertions.cs`](../../../API/SummerBornInfo.IntegrationTests/Assertions/LargeObjectAssertions.cs).

## Example

```csharp
public sealed class ImportSchoolsCommandHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenImportRequestCommand_WhenExecuted_ThenRequestIsSaved()
    {
        var dbContext = CreateDbContext();
        var handler = new ImportSchoolsCommandHandler(
            dbContext,
            new LargeObjectWriter(dbContext),
            new EventEmitter(dbContext));

        var command = new ImportSchoolsCommand(ExampleImportFile.GetExampleImportFileContent());

        var result = await handler.ExecuteAsync(command, TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, result.SchoolBulkImportRequestId);
    }
}
```

## When unsure

- Prefer a real database-backed test over a mocked repository test.
- Assert persistence, emitted events, and payload round-trips where relevant.
- If a new test helper is needed, keep it reusable and local to the existing test framework.
