---
name: architect
description: Decide where code belongs in this project, how to fit it into the vertical-slice structure, and how to wire new endpoints, commands, and queries.
---

# Architect

Use this skill for code layout, feature placement, and project pattern decisions.

## Rules

- Keep the vertical-slice structure intact.
- Put domain rules and entities in `API/SummerBornInfo.Domain`.
- Put commands, queries, handlers, and DTOs in `API/SummerBornInfo.Features`.
- Put EF Core, event queue, and storage implementations in `API/SummerBornInfo.Infrastructure`.
- Put route registration and HTTP wiring in `API/SummerBornInfo.Web`.
- Put test helpers and integration fixtures in the matching test project.
- Prefer existing patterns over new abstractions.

## Examples

### Register an endpoint

Follow [`API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs`](../../../API/SummerBornInfo.Web/API/Schools/SchoolEndpoints.cs):

```csharp
public static void RegisterSchoolEndpoints(this IEndpointRouteBuilder endpoints)
{
    var schools = endpoints.MapGroup("/api/schools");

    schools.MapGet("/", async (GetAllSchoolsQueryHandler handler, Guid? cursor, int? pageSize) =>
    {
        var query = new GetAllSchoolsQuery(cursor, pageSize ?? 100);
        var (items, nextCursor) = await handler.ExecuteAsync(query, CancellationToken.None);
        return Results.Ok(new { schools = items, nextCursor });
    });
}
```

### Add a query or command

Follow the request/handler split used by:

- [`API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQuery.cs`](../../../API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQuery.cs)
- [`API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQueryHandler.cs`](../../../API/SummerBornInfo.Features/Schools/Queries/GetAllSchools/GetAllSchoolsQueryHandler.cs)

```csharp
public sealed record GetAllSchoolsQuery(Guid? Cursor = null, int PageSize = 100);

public sealed class GetAllSchoolsQueryHandler(ApplicationDbContext context)
{
    public async Task<(List<SchoolDto> Schools, Guid? NextCursor)> ExecuteAsync(
        GetAllSchoolsQuery request,
        CancellationToken cancellationToken)
    {
        // Query the DbContext, map to DTOs, and return a result tuple.
    }
}
```

For commands, follow:

- [`API/SummerBornInfo.Features/Schools/Commands/Import/ImportSchoolsCommand.cs`](../../../API/SummerBornInfo.Features/Schools/Commands/Import/ImportSchoolsCommand.cs)
- [`API/SummerBornInfo.Features/Schools/Commands/Import/ImportSchoolsCommandHandler.cs`](../../../API/SummerBornInfo.Features/Schools/Commands/Import/ImportSchoolsCommandHandler.cs)

## When unsure

- Check project references before placing code.
- If code needs DI, add it in `API/SummerBornInfo.Web/Program.cs`.
- If the feature is a new slice, mirror the existing `Schools` structure first.

