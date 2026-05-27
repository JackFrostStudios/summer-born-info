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
        Assert.Equal(expectedSchool.Id, result.Id);
        Assert.Equal(expectedSchool.URN, result.URN);
        Assert.Equal(expectedSchool.UKPRN, result.UKPRN);
        Assert.Equal(expectedSchool.EstablishmentNumber, result.EstablishmentNumber);
        Assert.Equal(expectedSchool.Name, result.Name);
        Assert.Equal(expectedSchool.Address.Street, result.Address.Street);
        Assert.Equal(expectedSchool.Address.Locality, result.Address.Locality);
        Assert.Equal(expectedSchool.Address.AddressThree, result.Address.AddressThree);
        Assert.Equal(expectedSchool.Address.Town, result.Address.Town);
        Assert.Equal(expectedSchool.Address.County, result.Address.County);
        Assert.Equal(expectedSchool.Address.PostCode, result.Address.PostCode);
    }

    [Theory]
    [InlineData("/api/schools/search?urn=abc")]
    [InlineData("/api/schools/search?urn=")]
    [InlineData("/api/schools/search?urn=0")]
    public async Task GivenUrnIsInvalid_WhenGetSchoolByUrn_ThenReturnsBadRequest(string requestUri)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(requestUri, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenSchoolDoesNotExist_WhenGetSchoolByUrn_ThenReturnsNotFound()
    {
        await SeedSchoolsAsync(SchoolFactory.GetSchool());

        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/schools/search?urn=999999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
