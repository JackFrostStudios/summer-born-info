namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetAllSchoolsTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : SchoolEndpointTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GetAllSchoolsQuery()
    {
        var client = Factory.CreateClient();
        var result = await client.GetAsync("/api/schools", TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }
}
