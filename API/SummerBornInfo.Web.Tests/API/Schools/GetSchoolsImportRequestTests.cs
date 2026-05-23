namespace SummerBornInfo.Web.Tests.API.Schools;

public sealed class GetSchoolsImportRequestTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenUnauthenticatedCaller_WhenGetStatusByRequestId_ThenReturnsUnauthorized()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/admin/school-imports/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedNonAdminCaller_WhenGetStatusByRequestId_ThenReturnsForbidden()
    {
        var client = await CreateAuthenticatedTestClientAsync("volunteer@example.com", "P@ssword123!");

        var response = await client.GetAsync($"/api/admin/school-imports/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenSchoolBulkImportRequestExists_ThenReturnsExpectedPayload()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
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

        var response = await client.GetAsync($"/api/admin/school-imports/{requestId}", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GetSchoolBulkImportStatusResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(requestId, result.Id);
        Assert.Equal("CompletedWithFailures", result.Status);
        Assert.Equal(2, result.LinesProcessed);
        Assert.Equal(2, result.Failures.Count);
        Assert.Equal(2, result.Failures[0].LineNumber);
        Assert.Equal(10, result.Failures[1].LineNumber);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenGetStatusByRequestId_ThenExcludesContentIdFromPayload()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
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

        var response = await client.GetAsync($"/api/admin/school-imports/{requestId}", TestContext.Current.CancellationToken);

        _ = response.EnsureSuccessStatusCode();

        var rawPayload = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>(TestContext.Current.CancellationToken);
        Assert.NotNull(rawPayload);
        Assert.DoesNotContain(rawPayload.Keys, k => k.Equals("contentId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenSchoolBulkImportRequestMissing_ThenReturnsNotFound()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");

        var response = await client.GetAsync($"/api/admin/school-imports/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GivenAuthenticatedAdminCaller_WhenSchoolBulkImportRequestHasNoFailures_ThenReturnsEmptyFailuresArray()
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
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

        var response = await client.GetAsync($"/api/admin/school-imports/{requestId}", TestContext.Current.CancellationToken);

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
    public async Task GivenAuthenticatedAdminCaller_WhenSchoolBulkImportRequestInAnyStatus_ThenReturnsOk(SchoolBulkImportStatus status)
    {
        var client = await CreateAdminTestClientAsync("admin@example.com", "P@ssword123!");
        var requestId = Guid.CreateVersion7();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = CreateImportRequestInStatus(requestId, status);
            _ = dbContext.SchoolBulkImportRequests.Add(request);
            _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var response = await client.GetAsync($"/api/admin/school-imports/{requestId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static SchoolBulkImportRequest CreateImportRequestInStatus(Guid requestId, SchoolBulkImportStatus status)
    {
        SchoolBulkImportRequest request = new()
        {
            Id = requestId,
            ContentId = 999,
        };

        if (status is SchoolBulkImportStatus.Pending)
        {
            return request;
        }

        _ = request.ProcessingStarted();

        if (status is SchoolBulkImportStatus.Processing)
        {
            return request;
        }

        if (status is SchoolBulkImportStatus.Completed)
        {
            request.UpdateProgress(1, errorMessage: null);
            request.ProcessingComplete();
            return request;
        }

        if (status is SchoolBulkImportStatus.CompletedWithFailures)
        {
            request.UpdateProgress(1, "failure");
            request.ProcessingComplete();
            return request;
        }

        request.ProcessingFailed();
        return request;
    }
}
