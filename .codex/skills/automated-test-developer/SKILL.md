---
name: automated-test-developer
description: Design and implement automated tests for this project, choosing the right API or UI testing pattern for the changed surface.
---

# Automated Test Developer

Use this skill for test design and implementation.

## Rules

- Start by deciding whether the change belongs to `API/` or `UI/`, then follow the matching test strategy.
- Prefer API integration tests when the behavior touches EF Core, HTTP endpoints, events, or file/large-object storage.
- Prefer UI-focused tests for Angular rendering, routing, component interaction, and browser-facing state changes.
- Mock only dependencies that are truly external or unavailable in the test environment.
- Reuse the existing test framework before adding new helpers.
- Keep tests close to the feature they cover.
- Use the naming style and runner that match the surface under test:
  - API tests use xUnit and the current `Given_When_Then` naming style.
  - UI tests should follow the current Angular/Vitest setup and stay near the component or behaviour they cover.
- Design each test around inputs, outputs, post-action state, edge cases, and failure paths.
- Assert on outputs and system state, not on whether a function was called.
- Prefer black-box tests: verify that the input leads to the desired output or system state rather than checking internal implementation details.
- Aim for the smallest test set that still proves the behaviour.

## Existing API patterns

- Web integration tests inherit from [`API/SummerBornInfo.Web.Tests/TestFramework/WebIntegrationTestBase.cs`](../../../API/SummerBornInfo.Web.Tests/TestFramework/WebIntegrationTestBase.cs).
- Feature and infrastructure tests inherit from [`API/SummerBornInfo.TestFramework/IntegrationTestBase.cs`](../../../API/SummerBornInfo.TestFramework/IntegrationTestBase.cs).
- Database setup uses `IntegrationTestDatabaseServerFixture` and `IntegrationTestDatabaseInstanceFixture`.
- HTTP tests use `CustomWebApplicationFactory`.
- Event assertions live in [`API/SummerBornInfo.TestFramework/Assertions/EventAssertions.cs`](../../../API/SummerBornInfo.TestFramework/Assertions/EventAssertions.cs).
- Large object assertions live in [`API/SummerBornInfo.TestFramework/Assertions/LargeObjectAssertions.cs`](../../../API/SummerBornInfo.TestFramework/Assertions/LargeObjectAssertions.cs).

## Existing UI patterns

- Run UI tests from `UI/` with `npm test`.
- Keep tests close to the component, route, or behaviour they validate.
- Prefer focused tests around rendered output, route behaviour, and user interaction over implementation-detail assertions.
- When a UI change affects accessibility or semantics, include assertions or manual validation notes that cover the changed experience.

## API example

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

- Prefer a real database-backed API test over a mocked repository test when backend behaviour crosses persistence or transport boundaries.
- For UI work, prefer the smallest rendered-behaviour test that proves the user-visible outcome before reaching for broader end-to-end coverage.
- Assert persistence, emitted events, payload round-trips, rendered output, routing behaviour, accessibility impact, and other externally visible state where relevant.
- If a new test helper is needed, keep it reusable and local to the existing test framework for that surface.
- Return the minimal set of tests needed to validate the behaviour and the obvious edge cases.
