namespace SummerBornInfo.Web.Tests;

public class SchoolsIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAllSchoolsQuery()
    {
        var result = await _client.GetAsync("/api/schools", TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result.IsSuccessStatusCode);
    }
}