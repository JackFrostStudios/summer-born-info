namespace SummerBornInfo.Features.Tests.Schools.Queries.SearchSchools;

public sealed class SearchSchoolsQueryHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNameAndAddressMatches_WhenExecuteAsync_ThenRanksNameMatchAheadOfAddressOnlyMatch()
    {
        var nameMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            establishmentNumber: 3010,
            name: "Amber Hill School",
            street: "10 Cedar Road",
            locality: "Northside",
            addressThree: null,
            town: "York",
            county: "North Yorkshire",
            postCode: "YO1 1AA");
        var addressOnlyMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            establishmentNumber: 3020,
            name: "Northgate Primary",
            street: "20 Amber Street",
            locality: "Southside",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 2BB");

        await SeedSchoolsAsync(addressOnlyMatch, nameMatch);

        var handler = new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQueryHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQuery("amber", PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.Equal([nameMatch.Id, addressOnlyMatch.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenAddressAndPostcodeMatches_WhenExecuteAsync_ThenReturnsMatchingSchools()
    {
        var addressMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            establishmentNumber: 3010,
            name: "Northgate Primary",
            street: "1 Market Street",
            locality: "Old Town",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 1AA");
        var postcodeMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            establishmentNumber: 3020,
            name: "South Bank Academy",
            street: "2 River Road",
            locality: "Docklands",
            addressThree: null,
            town: "Hull",
            county: "East Yorkshire",
            postCode: "HU1 2BB");
        var nonMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000030"),
            urn: 100030,
            establishmentNumber: 3030,
            name: "Cedar Park School",
            street: "3 Oak Road",
            locality: "Westside",
            addressThree: null,
            town: "Bradford",
            county: "West Yorkshire",
            postCode: "BD1 3CC");

        await SeedSchoolsAsync(addressMatch, postcodeMatch, nonMatch);

        var handler = new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQueryHandler(CreateDbContext());

        var addressResult = await handler.ExecuteAsync(
            new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQuery("market street", PageSize: 10),
            TestContext.Current.CancellationToken);
        var postcodeResult = await handler.ExecuteAsync(
            new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQuery("hu12bb", PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.Equal([addressMatch.Id], [.. addressResult.Schools.Select(x => x.Id)]);
        Assert.Equal([postcodeMatch.Id], [.. postcodeResult.Schools.Select(x => x.Id)]);
    }

    [Fact]
    public async Task GivenNoSchoolsMatch_WhenExecuteAsync_ThenReturnsEmptyResponse()
    {
        await SeedSchoolsAsync(
            CreateSchool(
                id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
                urn: 100010,
                establishmentNumber: 3010,
                name: "Amber Hill School",
                street: "10 Cedar Road",
                locality: "Northside",
                addressThree: null,
                town: "York",
                county: "North Yorkshire",
                postCode: "YO1 1AA"));

        var handler = new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQueryHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new SummerBornInfo.Features.Schools.Queries.SearchSchools.SearchSchoolsQuery("nomatchvalue", PageSize: 10),
            TestContext.Current.CancellationToken);

        Assert.Empty(result.Schools);
        Assert.Null(result.NextCursor);
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        var dbContext = CreateDbContext();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static School CreateSchool(
        Guid id,
        int urn,
        int establishmentNumber,
        string name,
        string? street,
        string? locality,
        string? addressThree,
        string town,
        string? county,
        string postCode)
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
                Street = street,
                Locality = locality,
                AddressThree = addressThree,
                Town = town,
                County = county,
                PostCode = postCode,
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
