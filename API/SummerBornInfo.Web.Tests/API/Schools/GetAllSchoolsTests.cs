namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetAllSchoolsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolExists_WhenGetAllSchools_ThenReturnsSchoolInformation()
    {
        var expectedSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
            urn: 100001,
            ukprn: 200001,
            establishmentNumber: 3001,
            name: "Northbridge Primary",
            street: "1 Market Street",
            locality: "Old Town",
            addressThree: "Suite A",
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 1AA",
            openDate: new DateOnly(2010, 9, 1),
            closeDate: new DateOnly(2024, 7, 31));

        await SeedSchoolsAsync(expectedSchool);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools?pageSize=10", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var school = Assert.Single(result.Schools);
        Assert.Null(result.NextCursor);

        Assert.Equal(expectedSchool.Id, school.Id);
        Assert.Equal(expectedSchool.URN, school.URN);
        Assert.Equal(expectedSchool.UKPRN, school.UKPRN);
        Assert.Equal(expectedSchool.EstablishmentNumber, school.EstablishmentNumber);
        Assert.Equal(expectedSchool.Name, school.Name);
        Assert.Equal(expectedSchool.OpenDate, school.OpenDate);
        Assert.Equal(expectedSchool.CloseDate, school.CloseDate);

        Assert.Equal(expectedSchool.Address.Street, school.Address.Street);
        Assert.Equal(expectedSchool.Address.Locality, school.Address.Locality);
        Assert.Equal(expectedSchool.Address.AddressThree, school.Address.AddressThree);
        Assert.Equal(expectedSchool.Address.Town, school.Address.Town);
        Assert.Equal(expectedSchool.Address.County, school.Address.County);
        Assert.Equal(expectedSchool.Address.PostCode, school.Address.PostCode);

        Assert.Equal(expectedSchool.PhaseOfEducation.Id, school.PhaseOfEducation.Id);
        Assert.Equal(expectedSchool.PhaseOfEducation.Code, school.PhaseOfEducation.Code);
        Assert.Equal(expectedSchool.PhaseOfEducation.Name, school.PhaseOfEducation.Name);

        Assert.Equal(expectedSchool.LocalAuthority.Id, school.LocalAuthority.Id);
        Assert.Equal(expectedSchool.LocalAuthority.Code, school.LocalAuthority.Code);
        Assert.Equal(expectedSchool.LocalAuthority.Name, school.LocalAuthority.Name);

        Assert.Equal(expectedSchool.EstablishmentType.Code, school.EstablishmentType.Code);
        Assert.Equal(expectedSchool.EstablishmentType.Name, school.EstablishmentType.Name);
        Assert.Equal(expectedSchool.EstablishmentGroup.Code, school.EstablishmentGroup.Code);
        Assert.Equal(expectedSchool.EstablishmentGroup.Name, school.EstablishmentGroup.Name);
        Assert.Equal(expectedSchool.EstablishmentStatus.Code, school.EstablishmentStatus.Code);
        Assert.Equal(expectedSchool.EstablishmentStatus.Name, school.EstablishmentStatus.Name);
    }

    [Fact]
    public async Task GivenMoreSchoolsThanPageSize_WhenGetAllSchools_ThenReturnsPaginatedResults()
    {
        var firstSchool = CreateAmberHillSchool();
        var secondSchool = CreateBirchGroveSchool();
        var thirdSchool = CreateCedarParkSchool();

        await SeedSchoolsAsync(firstSchool, secondSchool, thirdSchool);

        var client = Factory.CreateClient();
        var firstResponse = await client.GetAsync("/api/schools?pageSize=2", TestContext.Current.CancellationToken);

        _ = firstResponse.EnsureSuccessStatusCode();
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.Schools.Count);
        Assert.Equal(firstSchool.Id, firstPage.Schools[0].Id);
        Assert.Equal(secondSchool.Id, firstPage.Schools[1].Id);
        Assert.True(firstPage.Schools[0].Id < firstPage.Schools[1].Id);
        Assert.Equal(secondSchool.Id.ToString(), firstPage.NextCursor);

        var secondResponse = await client.GetAsync($"/api/schools?pageSize=2&cursor={firstPage.NextCursor}", TestContext.Current.CancellationToken);

        _ = secondResponse.EnsureSuccessStatusCode();
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(secondPage);
        var remainingSchool = Assert.Single(secondPage.Schools);
        Assert.Equal(thirdSchool.Id, remainingSchool.Id);
        Assert.Equal(thirdSchool.Id.ToString(), remainingSchool.Id.ToString());
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task GivenPageSizeIsOmitted_WhenGetAllSchools_ThenReturnsDefaultPageSize()
    {
        var schools = CreateSequentialSchools(count: 101).ToArray();

        await SeedSchoolsAsync(schools);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(100, result.Schools.Count);
        Assert.Equal(schools[0].Id, result.Schools[0].Id);
        Assert.Equal(schools[99].Id, result.Schools[^1].Id);
        Assert.Equal(schools[99].Id.ToString(), result.NextCursor);
    }

    [Fact]
    public async Task GivenRequestedPageSizeExceedsMaximum_WhenGetAllSchools_ThenReturnsAtMostTwoHundredSchools()
    {
        var schools = CreateSequentialSchools(count: 201).ToArray();

        await SeedSchoolsAsync(schools);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools?pageSize=500", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(200, result.Schools.Count);
        Assert.Equal(schools[0].Id, result.Schools[0].Id);
        Assert.Equal(schools[199].Id, result.Schools[^1].Id);
        Assert.Equal(schools[199].Id.ToString(), result.NextCursor);
    }

    [Theory]
    [InlineData("/api/schools/search?pageSize=10")]
    [InlineData("/api/schools/search?q=amber&pageSize=10")]
    public async Task GivenDedicatedSchoolSearchRouteWithoutUrn_WhenRequested_ThenReturnsCollectionResponseShape(string requestUri)
    {
        var expectedSchool = CreateAmberHillSchool();

        await SeedSchoolsAsync(expectedSchool);

        var client = Factory.CreateClient();
        var response = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var school = Assert.Single(result.Schools);
        Assert.Equal(expectedSchool.Id, school.Id);
        Assert.Null(result.NextCursor);
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static School CreateAmberHillSchool()
    {
        return CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            ukprn: 200010,
            establishmentNumber: 3010,
            name: "Amber Hill School",
            street: "10 Cedar Road",
            locality: "Northside",
            addressThree: null,
            town: "York",
            county: "North Yorkshire",
            postCode: "YO1 1AA",
            openDate: new DateOnly(2012, 9, 1),
            closeDate: null);
    }

    private static School CreateBirchGroveSchool()
    {
        return CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            ukprn: 200020,
            establishmentNumber: 3020,
            name: "Birch Grove School",
            street: "20 Elm Road",
            locality: "Southside",
            addressThree: null,
            town: "Hull",
            county: "East Yorkshire",
            postCode: "HU1 2BB",
            openDate: new DateOnly(2014, 9, 1),
            closeDate: null);
    }

    private static School CreateCedarParkSchool()
    {
        return CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000030"),
            urn: 100030,
            ukprn: 200030,
            establishmentNumber: 3030,
            name: "Cedar Park School",
            street: "30 Oak Road",
            locality: "Westside",
            addressThree: "Building 3",
            town: "Bradford",
            county: "West Yorkshire",
            postCode: "BD1 3CC",
            openDate: new DateOnly(2016, 9, 1),
            closeDate: null);
    }

    private static IEnumerable<School> CreateSequentialSchools(int count)
    {
        for (var index = 1; index <= count; index++)
        {
            var indexText = index.ToString("D12", System.Globalization.CultureInfo.InvariantCulture);
            var nameText = index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
            var streetText = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var postCodeSuffix = (index % 10).ToString(System.Globalization.CultureInfo.InvariantCulture);

            yield return CreateSchool(
                id: new Guid($"00000000-0000-0000-0000-{indexText}"),
                urn: 100000 + index,
                ukprn: 200000 + index,
                establishmentNumber: 3000 + index,
                name: $"School {nameText}",
                street: $"{streetText} Test Street",
                locality: "Central",
                addressThree: null,
                town: "Leeds",
                county: "West Yorkshire",
                postCode: $"LS1 {postCodeSuffix}AA",
                openDate: new DateOnly(2010, 9, 1).AddDays(index),
                closeDate: null);
        }
    }

    private static School CreateSchool(
        Guid id,
        int urn,
        int? ukprn,
        int establishmentNumber,
        string name,
        string? street,
        string? locality,
        string? addressThree,
        string town,
        string? county,
        string postCode,
        DateOnly? openDate,
        DateOnly? closeDate)
    {
        var urnText = urn.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var phaseOfEducation = new PhaseOfEducation
        {
            Code = $"PRIMARY-{urnText}",
            Name = $"Primary {urnText}",
        };
        var localAuthority = new LocalAuthority
        {
            Code = $"LA-{urnText}",
            Name = $"Local Authority {urnText}",
        };
        var establishmentType = new EstablishmentType
        {
            Code = $"COMM-{urnText}",
            Name = $"Community school {urnText}",
        };
        var establishmentGroup = new EstablishmentGroup
        {
            Code = $"GROUP-{urnText}",
            Name = $"Local authority maintained schools {urnText}",
        };
        var establishmentStatus = new EstablishmentStatus
        {
            Code = $"OPEN-{urnText}",
            Name = $"Open {urnText}",
        };

        return new School
        {
            Id = id,
            URN = urn,
            UKPRN = ukprn,
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
            OpenDate = openDate,
            CloseDate = closeDate,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
        };
    }

}
