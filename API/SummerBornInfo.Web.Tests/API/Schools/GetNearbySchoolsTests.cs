namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetNearbySchoolsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : SchoolApiIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNearbySearchIsValid_WhenGetNearbySchools_ThenReturnsEmptyCollectionWrapper()
    {
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
    public async Task GivenNearbyCursorIsDecodable_WhenGetNearbySchools_ThenReturnsOk()
    {
        var client = Factory.CreateClient();
        var cursor = CreateCursor(
            """
            {"version":1,"latitude":53.8008,"longitude":-1.5491,"radiusMiles":5.0,"pageSize":10}
            """);

        var response = await client.GetAsync(
            $"/api/schools/nearby?latitude=53.8008&longitude=-1.5491&radiusMiles=5&pageSize=10&cursor={Uri.EscapeDataString(cursor)}",
            TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
    }

    [Theory]
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
}
