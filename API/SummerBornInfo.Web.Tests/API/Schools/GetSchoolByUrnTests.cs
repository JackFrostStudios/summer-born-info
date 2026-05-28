namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetSchoolByUrnTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolExists_WhenGetSchoolByUrn_ThenReturnsSchoolResponse()
    {
        var expectedSchool = SchoolFactory.GetSchool();
        expectedSchool.URN = 123456;
        expectedSchool.Name = "Northbridge Primary";

        await SeedSchoolsAsync(expectedSchool);

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?urn=123456", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SchoolResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        SchoolResponseAssertions.AssertMatches(expectedSchool, result);
    }

    [Theory]
    [InlineData("/api/schools/search?urn=abc")]
    [InlineData("/api/schools/search?urn=")]
    [InlineData("/api/schools/search?urn=0")]
    [InlineData("/api/schools/search?urn=123456&cursor=bad")]
    [InlineData("/api/schools/search?urn=123456&pageSize=0")]
    [InlineData("/api/schools/search?urn=123456&cursor=bad&pageSize=0")]
    public async Task GivenUrnIsInvalid_WhenGetSchoolByUrn_ThenReturnsBadRequest(string requestUri)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Invalid school discovery request.");
    }

    [Fact]
    public async Task GivenSchoolDoesNotExist_WhenGetSchoolByUrn_ThenReturnsNotFound()
    {
        await SeedSchoolsAsync(SchoolFactory.GetSchool());

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?urn=999999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "School not found.");
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatusCode, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
    }
}
