namespace SummerBornInfo.Features.Tests.Schools.Queries.GetAllSchools;

public sealed class GetAllSchoolsQueryHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolsExist_WhenExecuteAsyncWithoutCursor_ThenReturnsSchoolsOrderedByIdAndNoNextCursorWhenPageIsComplete()
    {
        // Arrange
        var secondSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            establishmentNumber: 3020,
            name: "Birch Grove School");
        var firstSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");

        await SeedSchoolsAsync(secondSchool, firstSchool);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(pageSize: 2),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([firstSchool.Id, secondSchool.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenMoreSchoolsThanRequestedPageSize_WhenExecuteAsync_ThenReturnsPageAndNextCursorFromLastReturnedSchool()
    {
        // Arrange
        var firstSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");
        var secondSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            establishmentNumber: 3020,
            name: "Birch Grove School");
        var thirdSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000030"),
            urn: 100030,
            establishmentNumber: 3030,
            name: "Cedar Park School");

        await SeedSchoolsAsync(firstSchool, thirdSchool, secondSchool);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(pageSize: 2),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([firstSchool.Id, secondSchool.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Equal(secondSchool.Id, result.NextCursor);
    }

    [Fact]
    public async Task GivenCursorIsProvided_WhenExecuteAsync_ThenReturnsOnlySchoolsWithIdGreaterThanCursor()
    {
        // Arrange
        var firstSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");
        var secondSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            establishmentNumber: 3020,
            name: "Birch Grove School");
        var thirdSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000030"),
            urn: 100030,
            establishmentNumber: 3030,
            name: "Cedar Park School");

        await SeedSchoolsAsync(firstSchool, secondSchool, thirdSchool);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(cursor: firstSchool.Id, pageSize: 5),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([secondSchool.Id, thirdSchool.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenPageSizeIsNullOrExceedsMaximum_WhenExecuteAsync_ThenUsesDefaultOrMaximumPageSize()
    {
        // Arrange
        var schools = CreateSequentialSchools(count: 201).ToArray();

        await SeedSchoolsAsync(schools);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var defaultPageResult = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(),
            TestContext.Current.CancellationToken);
        var maximumPageResult = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(pageSize: 500),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(GetAllSchoolsQuery.DefaultPageSize, defaultPageResult.Schools.Count);
        Assert.Equal(schools[GetAllSchoolsQuery.DefaultPageSize - 1].Id, defaultPageResult.NextCursor);
        Assert.Equal(GetAllSchoolsQuery.MaximumPageSize, maximumPageResult.Schools.Count);
        Assert.Equal(schools[GetAllSchoolsQuery.MaximumPageSize - 1].Id, maximumPageResult.NextCursor);
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        var dbContext = CreateDbContext();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static IEnumerable<School> CreateSequentialSchools(int count)
    {
        for (var index = 1; index <= count; index++)
        {
            var indexText = index.ToString("D12", System.Globalization.CultureInfo.InvariantCulture);
            yield return CreateSchool(
                id: new Guid($"00000000-0000-0000-0000-{indexText}"),
                urn: 100000 + index,
                establishmentNumber: 3000 + index,
                name: $"School {index:D3}");
        }
    }

    private static School CreateSchool(Guid id, int urn, int establishmentNumber, string name)
    {
        var urnText = urn.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return new School
        {
            Id = id,
            URN = urn,
            UKPRN = 200000 + urn,
            EstablishmentNumber = establishmentNumber,
            Name = name,
            Address = new SchoolAddress
            {
                Street = $"{urnText} Test Street",
                Locality = "Central",
                AddressThree = null,
                Town = "Leeds",
                County = "West Yorkshire",
                PostCode = $"LS1 {(urn % 10).ToString(System.Globalization.CultureInfo.InvariantCulture)}AA",
            },
            OpenDate = new DateOnly(2010, 9, 1),
            CloseDate = null,
            PhaseOfEducation = new PhaseOfEducation
            {
                Code = $"PRIMARY-{urnText}",
                Name = $"Primary {urnText}",
            },
            LocalAuthority = new LocalAuthority
            {
                Code = $"LA-{urnText}",
                Name = $"Local Authority {urnText}",
            },
            EstablishmentType = new EstablishmentType
            {
                Code = $"COMM-{urnText}",
                Name = $"Community school {urnText}",
            },
            EstablishmentGroup = new EstablishmentGroup
            {
                Code = $"GROUP-{urnText}",
                Name = $"Local authority maintained schools {urnText}",
            },
            EstablishmentStatus = new EstablishmentStatus
            {
                Code = $"OPEN-{urnText}",
                Name = $"Open {urnText}",
            },
        };
    }
}
