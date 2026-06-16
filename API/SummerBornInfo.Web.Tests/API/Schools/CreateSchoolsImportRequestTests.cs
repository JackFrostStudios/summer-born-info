using SharedFakeBritishNationalGridLocationConverter = SummerBornInfo.TestFramework.FakeBritishNationalGridLocationConverter;

namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class CreateSchoolsImportRequestTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(
        testDatabaseServerFixture,
        testOutputHelper,
        locationConverter: CreateLocationConverter())
{
    [Fact]
    public async Task GivenUnauthenticatedCaller_WhenSchoolImportPosted_ThenReturnsUnauthorized()
    {
        var client = Factory.CreateClient();
        using var content = CreateImportContent();

        var response = await client.PostAsync("/api/admin/school-imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, await GetImportRequestCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenAuthenticatedNonAdminCaller_WhenSchoolImportPosted_ThenReturnsForbidden()
    {
        var client = await CreateAuthenticatedTestClientAsync("volunteer@example.com", "P@ssword123!");
        using var content = CreateImportContent();

        var response = await client.PostAsync("/api/admin/school-imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, await GetImportRequestCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenSchoolImportPosted_ThenReturnsAcceptedAndBackgroundWorkerProcessesTheFile()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
        using var content = CreateImportContent();

        var response = await client.PostAsync("/api/admin/school-imports", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var importResponse = await response.Content.ReadFromJsonAsync<ImportSchoolsResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(importResponse);
        Assert.NotEqual(Guid.Empty, importResponse.Id);
        Assert.Equal("Pending", importResponse.Status);

        var request = await WaitForImportRequestAsync(importResponse.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(request);
        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schools = await dbContext.Schools
            .OrderBy(x => x.URN)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, schools.Count);
        AssertLocation(
            Assert.Single(schools, school => school.URN == 100000).Geometry,
            SharedFakeBritishNationalGridLocationConverter.CreateExampleAldgatePoint());
        AssertLocation(
            Assert.Single(schools, school => school.URN == 100004).Geometry,
            SharedFakeBritishNationalGridLocationConverter.CreateExampleSherbornePoint());
    }

    private static SharedFakeBritishNationalGridLocationConverter CreateLocationConverter()
    {
        return SharedFakeBritishNationalGridLocationConverter.ForExampleImportFile();
    }

    private static StreamContent CreateImportContent()
    {
        StreamContent content = new(ExampleImportFile.GetExampleImportFileContent());
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        return content;
    }

    private async Task<int> GetImportRequestCountAsync(CancellationToken cancellationToken)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.SchoolBulkImportRequests.CountAsync(cancellationToken);
    }

    private async Task<SchoolBulkImportRequest?> WaitForImportRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < TimeSpan.FromSeconds(15))
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = await dbContext.SchoolBulkImportRequests
                .Include(x => x.Failures)
                .SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);

            if (request?.Status is SchoolBulkImportStatus.Completed or SchoolBulkImportStatus.CompletedWithFailures or SchoolBulkImportStatus.Failed)
            {
                return request;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }

        return null;
    }

    private static void AssertLocation(Point? actualLocation, Point expectedLocation)
    {
        Assert.NotNull(actualLocation);
        Assert.Equal(expectedLocation.SRID, actualLocation.SRID);
        Assert.Equal(expectedLocation.X, actualLocation.X);
        Assert.Equal(expectedLocation.Y, actualLocation.Y);
    }
}
