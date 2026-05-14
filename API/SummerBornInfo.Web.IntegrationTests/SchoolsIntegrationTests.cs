using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using SummerBornInfo.Domain.Entities;
using SummerBornInfo.Features.Schools.Commands.Import;
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
        var payload = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("schoolBulkImportRequestId", out var idElement));
        Assert.Equal(requestId, idElement.GetGuid());
        Assert.Equal("CompletedWithFailures", root.GetProperty("status").GetString());
        Assert.Equal(2, root.GetProperty("linesProcessed").GetInt32());

        Assert.False(root.TryGetProperty("contentId", out _));

        var failures = root.GetProperty("failures");
        Assert.Equal(JsonValueKind.Array, failures.ValueKind);
        Assert.Equal(2, failures.GetArrayLength());
        Assert.Equal(2, failures[0].GetProperty("lineNumber").GetInt32());
        Assert.Equal(10, failures[1].GetProperty("lineNumber").GetInt32());
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
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var failures = document.RootElement.GetProperty("failures");
        Assert.Equal(JsonValueKind.Array, failures.ValueKind);
        Assert.Equal(0, failures.GetArrayLength());
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
