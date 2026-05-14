using System.Net.Http.Json;
using System.Net;
using SummerBornInfo.Domain.Entities;
using SummerBornInfo.Features.Schools.Commands.Import;
using SummerBornInfo.Features.Schools.Queries.GetSchoolBulkImportStatus;
using SummerBornInfo.TestFramework.TestData;

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

    [Fact]
    public async Task GivenImportRequest_WhenPosted_ThenBackgroundWorkerProcessesTheFile()
    {
        // Arrange
        var client = factory.CreateClient();
        using var csvStream = ExampleImportFile.GetExampleImportFileContent();
        using var content = new StreamContent(csvStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        // Act
        var response = await client.PostAsync("/api/schools/import", content, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var importResponse = await response.Content.ReadFromJsonAsync<ImportSchoolsResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(importResponse);

        var request = await WaitForImportRequestAsync(importResponse.SchoolBulkImportRequestId, TestContext.Current.CancellationToken);
        Assert.NotNull(request);
        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schools = await dbContext.Schools.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, schools.Count);
    }

    [Fact]
    public async Task GivenSchoolBulkImportRequest_WhenGetStatusByRequestId_ThenReturnsExpectedPayload()
    {
        // Arrange
        var client = factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = new SchoolBulkImportRequest
            {
                Id = requestId,
                ContentId = 42
            };

            request.ProcessingStarted();
            request.UpdateProgress(10, "Line 10 failed");
            request.UpdateProgress(2, "Line 2 failed");
            request.ProcessingComplete();

            dbContext.SchoolBulkImportRequests.Add(request);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GetSchoolBulkImportStatusResponse>(TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(requestId, result.SchoolBulkImportRequestId);
        Assert.Equal("CompletedWithFailures", result.Status);
        Assert.Equal(2, result.LinesProcessed);
        Assert.Equal(2, result.Failures.Count);
        Assert.Equal(2, result.Failures[0].LineNumber);
        Assert.Equal(10, result.Failures[1].LineNumber);

        var rawPayload = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>(TestContext.Current.CancellationToken);
        Assert.NotNull(rawPayload);
        Assert.False(rawPayload.ContainsKey("contentId"));
    }

    [Fact]
    public async Task GivenMissingSchoolBulkImportRequest_WhenGetStatusByRequestId_ThenReturnsNotFound()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/schools/import/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GivenSchoolBulkImportRequestWithoutFailures_WhenGetStatusByRequestId_ThenReturnsEmptyFailuresArray()
    {
        // Arrange
        var client = factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = new SchoolBulkImportRequest
            {
                Id = requestId,
                ContentId = 7
            };

            request.ProcessingStarted();
            request.UpdateProgress(1, null);
            request.ProcessingComplete();

            dbContext.SchoolBulkImportRequests.Add(request);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

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
        var client = factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var request = CreateImportRequestInStatus(requestId, status);
            dbContext.SchoolBulkImportRequests.Add(request);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var response = await client.GetAsync($"/api/schools/import/{requestId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<SchoolBulkImportRequest?> WaitForImportRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < TimeSpan.FromSeconds(15))
        {
            using var scope = factory.Services.CreateScope();
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

    private static SchoolBulkImportRequest CreateImportRequestInStatus(Guid requestId, SchoolBulkImportStatus status)
    {
        var request = new SchoolBulkImportRequest
        {
            Id = requestId,
            ContentId = 999
        };

        if (status is SchoolBulkImportStatus.Pending)
        {
            return request;
        }

        request.ProcessingStarted();

        if (status is SchoolBulkImportStatus.Processing)
        {
            return request;
        }

        if (status is SchoolBulkImportStatus.Completed)
        {
            request.UpdateProgress(1, null);
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
