namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetSchoolImportRequestTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : SchoolEndpointTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolBulkImportRequest_WhenGetStatusByRequestId_ThenReturnsExpectedPayload()
    {
        // Arrange
        var client = Factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SchoolBulkImportRequest request = new()
            {
                Id = requestId,
                ContentId = 42,
            };

            _ = request.ProcessingStarted();
            request.UpdateProgress(10, "Line 10 failed");
            request.UpdateProgress(2, "Line 2 failed");
            request.ProcessingComplete();

            _ = dbContext.SchoolBulkImportRequests.Add(request);
            _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GetSchoolBulkImportStatusResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(requestId, result.SchoolBulkImportRequestId);
        Assert.Equal("CompletedWithFailures", result.Status);
        Assert.Equal(2, result.LinesProcessed);
        Assert.Equal(2, result.Failures.Count);
        Assert.Equal(2, result.Failures[0].LineNumber);
        Assert.Equal(10, result.Failures[1].LineNumber);
    }

    [Fact]
    public async Task GivenSchoolBulkImportRequest_WhenGetStatusByRequestId_ThenExcludesContentIdFromPayload()
    {
        // Arrange
        var client = Factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SchoolBulkImportRequest request = new()
            {
                Id = requestId,
                ContentId = 42,
            };

            _ = dbContext.SchoolBulkImportRequests.Add(request);
            _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        _ = response.EnsureSuccessStatusCode();

        var rawPayload = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>(TestContext.Current.CancellationToken);
        Assert.NotNull(rawPayload);
        Assert.DoesNotContain(rawPayload.Keys, k => k.Equals("contentId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GivenMissingSchoolBulkImportRequest_WhenGetStatusByRequestId_ThenReturnsNotFound()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/schools/import/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GivenSchoolBulkImportRequestWithoutFailures_WhenGetStatusByRequestId_ThenReturnsEmptyFailuresArray()
    {
        // Arrange
        var client = Factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            SchoolBulkImportRequest request = new()
            {
                Id = requestId,
                ContentId = 7,
            };

            _ = request.ProcessingStarted();
            request.UpdateProgress(1, errorMessage: null);
            request.ProcessingComplete();

            _ = dbContext.SchoolBulkImportRequests.Add(request);
            _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GetSchoolBulkImportStatusResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Empty(result.Failures);
    }

    [Theory]
    [InlineData(SchoolBulkImportStatus.Pending)]
    [InlineData(SchoolBulkImportStatus.Processing)]
    [InlineData(SchoolBulkImportStatus.Completed)]
    [InlineData(SchoolBulkImportStatus.CompletedWithFailures)]
    [InlineData(SchoolBulkImportStatus.Failed)]
    public async Task GivenSchoolBulkImportRequestInAnyStatus_WhenGetStatusByRequestId_ThenReturnsOk(SchoolBulkImportStatus status)
    {
        // Arrange
        var client = Factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = CreateImportRequestInStatus(requestId, status);
            _ = dbContext.SchoolBulkImportRequests.Add(request);
            _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
