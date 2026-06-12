namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetNearbySchoolsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNearbySearchHasNoMatches_WhenGetNearbySchools_ThenReturnsEmptyCollectionWrapper()
    {
        await SeedSchoolsAsync(
            CreateSchool(
                id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x10) /* 00000000-0000-0000-0000-000000000010 */,
                urn: 100010,
                ukprn: 200010,
                establishmentNumber: 3010,
                name: "Distant School",
                street: "10 Remote Road",
                locality: "Farfield",
                addressThree: null,
                town: "Newcastle",
                county: "Tyne and Wear",
                postCode: "NE1 1AA",
                latitude: 54.9783,
                longitude: -1.6178));

        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            "/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=10",
            TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result.Schools);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GivenNearbyMatchesExist_WhenGetNearbySchools_ThenReturnsFirstPageOrderedByDistanceAndId()
    {
        var scenario = CreateNearbyOrderingScenario();

        await SeedSchoolsAsync(
            scenario.OutsideRadiusSchool,
            scenario.TiedThirdSchool,
            scenario.MissingLocationSchool,
            scenario.ClosestSchool,
            scenario.TiedSecondSchool);

        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            "/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=2",
            TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal([scenario.ClosestSchool.Id, scenario.TiedSecondSchool.Id], [.. result.Schools.Select(school => school.Id)]);
        SchoolResponseAssertions.AssertMatches(scenario.ClosestSchool, result.Schools[0]);
        SchoolResponseAssertions.AssertMatches(scenario.TiedSecondSchool, result.Schools[1]);
        Assert.All(result.Schools, school => Assert.NotNull(school.Latitude));
        Assert.DoesNotContain(result.Schools, school => school.Id == scenario.MissingLocationSchool.Id);
        Assert.DoesNotContain(result.Schools, school => school.Id == scenario.OutsideRadiusSchool.Id);
    }

    [Fact]
    public async Task GivenNearbyMatchesSpanMultiplePages_WhenFollowingNextCursor_ThenReturnsStableContinuation()
    {
        var scenario = CreateNearbyOrderingScenario();

        await SeedSchoolsAsync(
            scenario.OutsideRadiusSchool,
            scenario.TiedThirdSchool,
            scenario.MissingLocationSchool,
            scenario.ClosestSchool,
            scenario.TiedSecondSchool);

        var client = Factory.CreateClient();

        var firstResponse = await client.GetAsync(
            "/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=2",
            TestContext.Current.CancellationToken);

        _ = firstResponse.EnsureSuccessStatusCode();
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(firstPage);
        Assert.Equal([scenario.ClosestSchool.Id, scenario.TiedSecondSchool.Id], [.. firstPage.Schools.Select(school => school.Id)]);
        Assert.False(string.IsNullOrWhiteSpace(firstPage.NextCursor));

        var secondResponse = await client.GetAsync(
            $"/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=2&cursor={Uri.EscapeDataString(firstPage.NextCursor)}",
            TestContext.Current.CancellationToken);

        _ = secondResponse.EnsureSuccessStatusCode();
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<SchoolsResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(secondPage);
        Assert.Equal([scenario.TiedThirdSchool.Id], [.. secondPage.Schools.Select(school => school.Id)]);
        SchoolResponseAssertions.AssertMatches(scenario.TiedThirdSchool, Assert.Single(secondPage.Schools));
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task GivenNearbyCursorIsDecodableAndCompatible_WhenGetNearbySchools_ThenReturnsOk()
    {
        var client = Factory.CreateClient();
        var cursor = CreateCursor(
            /*lang=json,strict*/
            """
            {"version":1,"latitude":53.8008,"longitude":-1.5491,"radiusMiles":5.0,"pageSize":10,"distanceMeters":123.45,"schoolId":"00000000-0000-0000-0000-000000000010"}
            """
        );

        var response = await client.GetAsync(
            $"/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=10&cursor={Uri.EscapeDataString(cursor)}",
            TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("/api/schools/nearby?latitude=53.8009&longitude=-1.5491&radiusMiles=5&pageSize=10&cursor={0}")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5492&radiusMiles=5&pageSize=10&cursor={0}")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=6&pageSize=10&cursor={0}")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=9&cursor={0}")]
    public async Task GivenNearbyCursorReplayIsIncompatible_WhenGetNearbySchools_ThenReturnsBadRequest(string requestUriTemplate)
    {
        var client = Factory.CreateClient();
        var cursor = CreateCursor(
            /*lang=json,strict*/
            """
            {"version":1,"latitude":53.8008,"longitude":-1.5491,"radiusMiles":5.0,"pageSize":10,"distanceMeters":123.45,"schoolId":"00000000-0000-0000-0000-000000000010"}
            """
        );

        var response = await client.GetAsync(
            string.Format(CultureInfo.InvariantCulture, requestUriTemplate, Uri.EscapeDataString(cursor)),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response);
    }

    [Theory]
    [InlineData("/api/schools/nearby?longitude=-1.5491&radiusMiles=5")]
    [InlineData("/api/schools/nearby?latitude=53.8008&radiusMiles=5")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491")]
    [InlineData("/api/schools/nearby")]
    [InlineData("/api/schools/nearby?latitude=91&longitude=-1.5491&radiusMiles=5")]
    [InlineData("/api/schools/nearby?latitude=-91&longitude=-1.5491&radiusMiles=5")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=181&radiusMiles=5")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-181&radiusMiles=5")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=0")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=-1")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=100.1")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=0")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=201")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&cursor=")]
    [InlineData("/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&cursor=not-a-valid-cursor")]
    public async Task GivenNearbyInputIsInvalid_WhenGetNearbySchools_ThenReturnsBadRequest(string requestUri)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response);
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static NearbyOrderingScenario CreateNearbyOrderingScenario()
    {
        return new NearbyOrderingScenario(
            CreateClosestNearbySchool(),
            CreateTiedSecondNearbySchool(),
            CreateTiedThirdNearbySchool(),
            CreateOutsideRadiusNearbySchool(),
            CreateMissingLocationNearbySchool());
    }

    private static School CreateClosestNearbySchool()
    {
        return CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x10) /* 00000000-0000-0000-0000-000000000010 */,
            urn: 100010,
            ukprn: 200010,
            establishmentNumber: 3010,
            name: "City Centre School",
            street: "1 Market Street",
            locality: "Centre",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 1AA",
            latitude: 53.8008,
            longitude: -1.5491);
    }

    private static School CreateTiedSecondNearbySchool()
    {
        return CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20) /* 00000000-0000-0000-0000-000000000020 */,
            urn: 100020,
            ukprn: 200020,
            establishmentNumber: 3020,
            name: "North Leeds School",
            street: "2 Park Lane",
            locality: "North",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 2AA",
            latitude: 53.8100,
            longitude: -1.5400);
    }

    private static School CreateTiedThirdNearbySchool()
    {
        return CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x30) /* 00000000-0000-0000-0000-000000000030 */,
            urn: 100030,
            ukprn: 200030,
            establishmentNumber: 3030,
            name: "North Leeds Annex",
            street: "3 Park Lane",
            locality: "North",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 3AA",
            latitude: 53.8100,
            longitude: -1.5400);
    }

    private static School CreateOutsideRadiusNearbySchool()
    {
        return CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x40) /* 00000000-0000-0000-0000-000000000040 */,
            urn: 100040,
            ukprn: 200040,
            establishmentNumber: 3040,
            name: "Bradford School",
            street: "4 Valley Road",
            locality: "West",
            addressThree: null,
            town: "Bradford",
            county: "West Yorkshire",
            postCode: "BD1 1AA",
            latitude: 53.7950,
            longitude: -1.7594);
    }

    private static School CreateMissingLocationNearbySchool()
    {
        return CreateSchool(
            id: new Guid(0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x50) /* 00000000-0000-0000-0000-000000000050 */,
            urn: 100050,
            ukprn: 200050,
            establishmentNumber: 3050,
            name: "Location Pending School",
            street: "5 Unknown Road",
            locality: "Unknown",
            addressThree: null,
            town: "Leeds",
            county: "West Yorkshire",
            postCode: "LS1 4AA",
            latitude: null,
            longitude: null);
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
        double? latitude,
        double? longitude)
    {
        var urnText = urn.ToString(CultureInfo.InvariantCulture);

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
            Geometry = latitude.HasValue && longitude.HasValue
                ? new Point(longitude.Value, latitude.Value) { SRID = 4326 }
                : null,
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

    private static string CreateCursor(string json)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
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

    private sealed record NearbyOrderingScenario(
        School ClosestSchool,
        School TiedSecondSchool,
        School TiedThirdSchool,
        School OutsideRadiusSchool,
        School MissingLocationSchool);
}
