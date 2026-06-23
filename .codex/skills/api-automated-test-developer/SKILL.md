---
name: api-automated-test-developer
description: Design and implement automated tests for API work in this project, favoring the existing xUnit and integration-test patterns. Use when changes belong in `API/` and need test coverage or test design guidance.
---

# API Automated Test Developer

Use this skill for API test design and implementation.

## Rules

- Prefer integration tests when the behavior touches EF Core, HTTP endpoints, events, or file/large-object storage.
- Mock only dependencies that are truly external or unavailable in the test environment.
- Reuse the existing API test framework before adding new helpers.
- Keep tests close to the feature they cover.
- Use xUnit and the project's current `Given_When_Then` naming style.
- Design each test around inputs, outputs, post-action state, edge cases, and failure paths.
- Assert on outputs and system state, not on whether a function was called.
- Prefer black-box tests: verify that the input leads to the desired output or system state rather than checking internal implementation details.
- Aim for the smallest test set that still proves the behaviour.

## Existing patterns

- Web integration tests inherit from [`API/SummerBornInfo.Web.Tests/TestFramework/WebIntegrationTestBase.cs`](../../../API/SummerBornInfo.Web.Tests/TestFramework/WebIntegrationTestBase.cs).
- Feature and infrastructure tests inherit from [`API/SummerBornInfo.TestFramework/IntegrationTestBase.cs`](../../../API/SummerBornInfo.TestFramework/IntegrationTestBase.cs).
- Database setup uses `IntegrationTestDatabaseServerFixture` and `IntegrationTestDatabaseInstanceFixture`.
- HTTP tests use `CustomWebApplicationFactory`.
- Event assertions live in [`API/SummerBornInfo.TestFramework/Assertions/EventAssertions.cs`](../../../API/SummerBornInfo.TestFramework/Assertions/EventAssertions.cs).
- Large object assertions live in [`API/SummerBornInfo.TestFramework/Assertions/LargeObjectAssertions.cs`](../../../API/SummerBornInfo.TestFramework/Assertions/LargeObjectAssertions.cs).

## Example

```csharp
public sealed class ImportSchoolsCommandHandlerTests : IntegrationTestBase
{
    [Fact]
    public async Task GivenValidImportFile_WhenExecuted_ThenRequestIsSaved()
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

- Prefer a real database-backed test over a mocked repository test when backend behaviour crosses persistence or transport boundaries.
- Assert persistence, emitted events, payload round-trips, and other externally visible state where relevant.
- If a new test helper is needed, keep it reusable and local to the existing API test framework.
- Return the minimal set of tests needed to validate the behaviour and the obvious edge cases.
