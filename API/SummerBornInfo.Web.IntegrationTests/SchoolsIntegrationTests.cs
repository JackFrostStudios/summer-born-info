namespace SummerBornInfo.Web.Tests;

public sealed class SchoolsIntegrationTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) 
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GetAllSchoolsQuery()
    {
        var client = factory.CreateClient();
        var result = await client.GetAsync("/api/schools", TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }
}