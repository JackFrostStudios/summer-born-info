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
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20) /* 00000000-0000-0000-0000-000000000020 */,
            urn: 100020,
            establishmentNumber: 3020,
            name: "Birch Grove School");
        var firstSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x10) /* 00000000-0000-0000-0000-000000000010 */,
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");

        await SeedSchoolsAsync(secondSchool, firstSchool);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(PageSize: 2),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([firstSchool.Id, secondSchool.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenSchoolExists_WhenExecuteAsync_ThenReturnsSharedSchoolResponseShape()
    {
        // Arrange
        var school = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x10) /* 00000000-0000-0000-0000-000000000010 */,
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");

        await SeedSchoolsAsync(school);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(PageSize: 1),
            TestContext.Current.CancellationToken);

        // Assert
        var response = Assert.Single(result.Schools);
        Assert.Equal(school.Id, response.Id);
        Assert.Equal(school.URN, response.URN);
        Assert.Equal(school.UKPRN, response.UKPRN);
        Assert.Equal(school.EstablishmentNumber, response.EstablishmentNumber);
        Assert.Equal(school.Name, response.Name);
        Assert.Equal(school.OpenDate, response.OpenDate);
        Assert.Equal(school.CloseDate, response.CloseDate);
        Assert.Equal(school.Address.Street, response.Address.Street);
        Assert.Equal(school.Address.Locality, response.Address.Locality);
        Assert.Equal(school.Address.AddressThree, response.Address.AddressThree);
        Assert.Equal(school.Address.Town, response.Address.Town);
        Assert.Equal(school.Address.County, response.Address.County);
        Assert.Equal(school.Address.PostCode, response.Address.PostCode);
        Assert.Equal(school.PhaseOfEducation.Id, response.PhaseOfEducation.Id);
        Assert.Equal(school.PhaseOfEducation.Code, response.PhaseOfEducation.Code);
        Assert.Equal(school.PhaseOfEducation.Name, response.PhaseOfEducation.Name);
        Assert.Equal(school.LocalAuthority.Id, response.LocalAuthority.Id);
        Assert.Equal(school.LocalAuthority.Code, response.LocalAuthority.Code);
        Assert.Equal(school.LocalAuthority.Name, response.LocalAuthority.Name);
        Assert.Equal(school.EstablishmentType.Id, response.EstablishmentType.Id);
        Assert.Equal(school.EstablishmentType.Code, response.EstablishmentType.Code);
        Assert.Equal(school.EstablishmentType.Name, response.EstablishmentType.Name);
        Assert.Equal(school.EstablishmentGroup.Id, response.EstablishmentGroup.Id);
        Assert.Equal(school.EstablishmentGroup.Code, response.EstablishmentGroup.Code);
        Assert.Equal(school.EstablishmentGroup.Name, response.EstablishmentGroup.Name);
        Assert.Equal(school.EstablishmentStatus.Id, response.EstablishmentStatus.Id);
        Assert.Equal(school.EstablishmentStatus.Code, response.EstablishmentStatus.Code);
        Assert.Equal(school.EstablishmentStatus.Name, response.EstablishmentStatus.Name);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenMoreSchoolsThanRequestedPageSize_WhenExecuteAsync_ThenReturnsPageAndNextCursorFromLastReturnedSchool()
    {
        // Arrange
        var firstSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x10) /* 00000000-0000-0000-0000-000000000010 */,
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");
        var secondSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20) /* 00000000-0000-0000-0000-000000000020 */,
            urn: 100020,
            establishmentNumber: 3020,
            name: "Birch Grove School");
        var thirdSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x30) /* 00000000-0000-0000-0000-000000000030 */,
            urn: 100030,
            establishmentNumber: 3030,
            name: "Cedar Park School");

        await SeedSchoolsAsync(firstSchool, thirdSchool, secondSchool);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(PageSize: 2),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal([firstSchool.Id, secondSchool.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Equal(secondSchool.Id.ToString(), result.NextCursor);
    }

    [Fact]
    public async Task GivenCursorIsProvided_WhenExecuteAsync_ThenReturnsOnlySchoolsWithIdGreaterThanCursor()
    {
        // Arrange
        var firstSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x10) /* 00000000-0000-0000-0000-000000000010 */,
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School");
        var secondSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20) /* 00000000-0000-0000-0000-000000000020 */,
            urn: 100020,
            establishmentNumber: 3020,
            name: "Birch Grove School");
        var thirdSchool = CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x30) /* 00000000-0000-0000-0000-000000000030 */,
            urn: 100030,
            establishmentNumber: 3030,
            name: "Cedar Park School");

        await SeedSchoolsAsync(firstSchool, secondSchool, thirdSchool);

        var handler = new GetAllSchoolsQueryHandler(CreateDbContext());

        // Act
        var result = await handler.ExecuteAsync(
            new GetAllSchoolsQuery(Cursor: firstSchool.Id, PageSize: 5),
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
            new GetAllSchoolsQuery(PageSize: 500),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(GetAllSchoolsQuery.DefaultPageSize, defaultPageResult.Schools.Count);
        Assert.Equal(schools[GetAllSchoolsQuery.DefaultPageSize - 1].Id.ToString(), defaultPageResult.NextCursor);
        Assert.Equal(GetAllSchoolsQuery.MaximumPageSize, maximumPageResult.Schools.Count);
        Assert.Equal(schools[GetAllSchoolsQuery.MaximumPageSize - 1].Id.ToString(), maximumPageResult.NextCursor);
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
            var nameText = index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
            yield return CreateSchool(
                id: new Guid($"00000000-0000-0000-0000-{indexText}"),
                urn: 100000 + index,
                establishmentNumber: 3000 + index,
                name: $"School {nameText}");
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
                PostCode = string.Create(
                    System.Globalization.CultureInfo.InvariantCulture,
                    $"LS1 {urn % 10}AA"),
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
