namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class SearchSchoolsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNameAndAddressMatches_WhenGetSchoolsSearch_ThenReturnsRankedResults()
    {
        var nameMatch = CreateSchool(
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
            postCode: "YO1 1AA");
        var addressOnlyMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
            urn: 100020,
            ukprn: 200020,
            establishmentNumber: 3020,
            name: "Northgate Primary",
            street: "20 Amber Street",
            locality: "Southside",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 2BB");

        await SeedSchoolsAsync(addressOnlyMatch, nameMatch);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?q=amber&pageSize=10", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal([nameMatch.Id, addressOnlyMatch.Id], [.. result.Schools.Select(x => x.Id)]);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenMoreRankedMatchesThanPageSize_WhenGetSchoolsSearch_ThenReturnsStableContinuationAcrossPages()
    {
        var (firstSchool, secondSchool, thirdSchool) = CreateAmberPaginationSchools();

        await SeedSchoolsAsync(thirdSchool, firstSchool, secondSchool);

        var client = Factory.CreateClient();
        var firstResponse = await client.GetAsync("/api/schools/search?q=amber&pageSize=2", TestContext.Current.CancellationToken);

        _ = firstResponse.EnsureSuccessStatusCode();
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(firstPage);
        Assert.Equal([firstSchool.Id, secondSchool.Id], [.. firstPage.Schools.Select(x => x.Id)]);
        Assert.NotNull(firstPage.NextCursor);
        Assert.DoesNotContain(firstPage.Schools[1].Id.ToString(), firstPage.NextCursor, StringComparison.OrdinalIgnoreCase);

        var secondResponse = await client.GetAsync(
            $"/api/schools/search?q=amber&pageSize=2&cursor={Uri.EscapeDataString(firstPage.NextCursor)}",
            TestContext.Current.CancellationToken);

        _ = secondResponse.EnsureSuccessStatusCode();
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(secondPage);
        var remainingSchool = Assert.Single(secondPage.Schools);
        Assert.Equal(thirdSchool.Id, remainingSchool.Id);
        Assert.Null(secondPage.NextCursor);
    }

    [Theory]
    [InlineData("market street", "00000000-0000-0000-0000-000000000010")]
    [InlineData("hu12bb", "00000000-0000-0000-0000-000000000020")]
    public async Task GivenAddressOrPostcodeMatches_WhenGetSchoolsSearch_ThenReturnsMatchingSchools(string query, string expectedSchoolId)
    {
        var addressMatch = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
            urn: 100010,
            ukprn: 200010,
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
            ukprn: 200020,
            establishmentNumber: 3020,
            name: "South Bank Academy",
            street: "2 River Road",
            locality: "Docklands",
            addressThree: null,
            town: "Hull",
            county: "East Yorkshire",
            postCode: "HU1 2BB");

        await SeedSchoolsAsync(addressMatch, postcodeMatch);

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/api/schools/search?q={Uri.EscapeDataString(query)}", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var school = Assert.Single(result.Schools);
        Assert.Equal(Guid.Parse(expectedSchoolId), school.Id);
    }

    [Fact]
    public async Task GivenTypoTolerantNameMatch_WhenGetSchoolsSearch_ThenReturnsMatchingSchool()
    {
        var expectedSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000050"),
            urn: 100050,
            ukprn: 200050,
            establishmentNumber: 3050,
            name: "Amber Hill School",
            street: "50 Cedar Road",
            locality: "Northside",
            addressThree: null,
            town: "York",
            county: "North Yorkshire",
            postCode: "YO1 1AD");

        await SeedSchoolsAsync(expectedSchool);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?q=ambr", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var school = Assert.Single(result.Schools);
        Assert.Equal(expectedSchool.Id, school.Id);
    }

    [Fact]
    public async Task GivenNoSchoolsMatch_WhenGetSchoolsSearch_ThenReturnsEmptyPage()
    {
        await SeedSchoolsAsync(
            CreateSchool(
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
                postCode: "YO1 1AA"));

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?q=nomatchvalue", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result.Schools);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenMatchingSchoolExists_WhenGetSchoolsSearch_ThenReturnsCollectionWrapperWithFullSchoolShape()
    {
        var expectedSchool = CreateSchool(
            id: Guid.Parse("00000000-0000-0000-0000-000000000040"),
            urn: 100040,
            ukprn: 200040,
            establishmentNumber: 3040,
            name: "Harbour View Academy",
            street: "40 Anchor Lane",
            locality: "Waterfront",
            addressThree: "Building 4",
            town: "Liverpool",
            county: "Merseyside",
            postCode: "L1 4AB");

        await SeedSchoolsAsync(expectedSchool);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?q=harbour", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Null(result.NextCursor);

        var school = Assert.Single(result.Schools);
        SchoolResponseAssertions.AssertMatches(expectedSchool, school);
    }

    [Theory]
    [InlineData("/api/schools/search")]
    [InlineData("/api/schools/search?q=")]
    [InlineData("/api/schools/search?q=   ")]
    [InlineData("/api/schools/search?q=abc")]
    [InlineData("/api/schools/search?q=amber&urn=123456")]
    [InlineData("/api/schools/search?q=amber&pageSize=0")]
    [InlineData("/api/schools/search?q=amber&pageSize=201")]
    [InlineData("/api/schools/search?q=amber&cursor=not-a-valid-cursor")]
    public async Task GivenSearchInputIsInvalid_WhenGetSchoolsSearch_ThenReturnsBadRequest(string requestUri)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response);
    }

    [Fact]
    public async Task GivenCursorDoesNotMatchQuery_WhenGetSchoolsSearch_ThenReturnsBadRequest()
    {
        await SeedSchoolsAsync(CreateCursorCompatibilitySchools());

        var client = Factory.CreateClient();
        var firstResponse = await client.GetAsync("/api/schools/search?q=amber&pageSize=1", TestContext.Current.CancellationToken);

        _ = firstResponse.EnsureSuccessStatusCode();
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(firstPage);
        Assert.NotNull(firstPage.NextCursor);

        var incompatibleResponse = await client.GetAsync(
            $"/api/schools/search?q=cedar&pageSize=1&cursor={Uri.EscapeDataString(firstPage.NextCursor)}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, incompatibleResponse.StatusCode);
        await AssertProblemDetailsAsync(incompatibleResponse);
    }

    private static (School FirstSchool, School SecondSchool, School ThirdSchool) CreateAmberPaginationSchools()
    {
        return (
            CreateSchool(
                id: Guid.Parse("00000000-0000-0000-0000-000000000010"),
                urn: 100010,
                ukprn: 200010,
                establishmentNumber: 3010,
                name: "Amber Alpha School",
                street: "1 Cedar Road",
                locality: "Northside",
                addressThree: null,
                town: "York",
                county: "North Yorkshire",
                postCode: "YO1 1AA"),
            CreateSchool(
                id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
                urn: 100020,
                ukprn: 200020,
                establishmentNumber: 3020,
                name: "Amber Beta School",
                street: "2 Cedar Road",
                locality: "Northside",
                addressThree: null,
                town: "York",
                county: "North Yorkshire",
                postCode: "YO1 1AB"),
            CreateSchool(
                id: Guid.Parse("00000000-0000-0000-0000-000000000030"),
                urn: 100030,
                ukprn: 200030,
                establishmentNumber: 3030,
                name: "Amber Gamma School",
                street: "3 Cedar Road",
                locality: "Northside",
                addressThree: null,
                town: "York",
                county: "North Yorkshire",
                postCode: "YO1 1AC"));
    }

    private static School[] CreateCursorCompatibilitySchools()
    {
        return
        [
            CreateSchool(
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
                postCode: "YO1 1AA"),
            CreateSchool(
                id: Guid.Parse("00000000-0000-0000-0000-000000000020"),
                urn: 100020,
                ukprn: 200020,
                establishmentNumber: 3020,
                name: "Amber Valley School",
                street: "20 Cedar Road",
                locality: "Northside",
                addressThree: null,
                town: "York",
                county: "North Yorkshire",
                postCode: "YO1 1AB"),
            CreateSchool(
                id: Guid.Parse("00000000-0000-0000-0000-000000000030"),
                urn: 100030,
                ukprn: 200030,
                establishmentNumber: 3030,
                name: "Cedar Park School",
                street: "30 Oak Road",
                locality: "Westside",
                addressThree: null,
                town: "Leeds",
                county: "West Yorkshire",
                postCode: "LS1 2BB"),
        ];
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
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
        string postCode)
    {
        var urnText = urn.ToString(System.Globalization.CultureInfo.InvariantCulture);

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

    private static async Task AssertProblemDetailsAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Invalid school discovery request.", problem.Title);
    }
}
