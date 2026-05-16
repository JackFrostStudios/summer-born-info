namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile;

public sealed class ProcessImportFileCommandHandlerTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenMissingImportRequest_WhenExecuted_ThenExceptionIsThrown()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var handler = CreateHandler(dbContext);
        ProcessImportFileCommand command = new(Guid.CreateVersion7());

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.ExecuteAsync(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenMissingLargeObject_WhenExecuted_ThenExceptionIsThrown()
    {
        // Arrange
        var dbContext = CreateDbContext();
        SchoolBulkImportRequest schoolBulkImportRequest = new()
        {
            ContentId = 999999,
        };

        dbContext.SchoolBulkImportRequests.Add(schoolBulkImportRequest);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = CreateHandler(CreateDbContext());
        ProcessImportFileCommand command = new(schoolBulkImportRequest.Id);

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.ExecuteAsync(command, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenValidImportRequest_WhenExecuted_ThenProgressAndStatusAreUpdated()
    {
        // Arrange
        var requestId = await CreateImportRequestAsync(ExampleImportFile.GetExampleImportFileContent());
        var handler = CreateHandler(CreateDbContext());

        // Act
        await handler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken);

        // Assert
        var verifyDbContext = CreateDbContext();
        var request = await verifyDbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);

        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        var schools = await verifyDbContext.Schools.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, schools.Count);
    }

    [Fact]
    public async Task GivenImportRequestWithInvalidRow_WhenExecuted_ThenFailureIsRecordedAndImportContinues()
    {
        await
                // Arrange
                using var invalidCsv = CreateCsvStream(
            "\"URN\",\"EstablishmentNumber\",\"EstablishmentName\",\"LA (code)\",\"LA (name)\",\"TypeOfEstablishment (code)\",\"TypeOfEstablishment (name)\",\"EstablishmentTypeGroup (code)\",\"EstablishmentTypeGroup (name)\",\"EstablishmentStatus (code)\",\"EstablishmentStatus (name)\",\"PhaseOfEducation (code)\",\"PhaseOfEducation (name)\",\"OpenDate\",\"CloseDate\",\"UKPRN\",\"Street\",\"Locality\",\"Address3\",\"Town\",\"County (name)\",\"Postcode\"",
            "\"100000\",\"3614\",\"The Aldgate School\",\"201\",\"City of London\",\"02\",\"Voluntary aided school\",\"4\",\"Local authority maintained schools\",\"1\",\"Open\",\"2\",\"Primary\",\"\",\"\",\"10079319\",\"St James's Passage\",\"Duke's Place\",\"\",\"London\",\"\",\"EC3A 5DE\"",
            "\"INVALID\",\"1045\",\"Broken School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"",
            "\"100004\",\"1045\",\"Sherborne Nursery School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"");
        var requestId = await CreateImportRequestAsync(invalidCsv);
        var handler = CreateHandler(CreateDbContext());

        // Act
        await handler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken);

        // Assert
        var verifyDbContext = CreateDbContext();
        var request = await verifyDbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);

        Assert.Equal(3, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures, request.Status);
        Assert.Single(request.Failures);
        Assert.Equal(3, request.Failures[0].LineNumber);
        Assert.NotNull(request.Failures[0].ErrorMessage);

        var schools = await verifyDbContext.Schools
            .OrderBy(x => x.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([100000, 100004], schools.Select(x => x.URN).ToArray());
    }

    [Fact]
    public async Task GivenCompletedImportRequest_WhenExecuted_ThenRequestIsNotProcessedAgain()
    {
        // Arrange
        var requestId = await CreateImportRequestAsync(ExampleImportFile.GetExampleImportFileContent());
        var initialDbContext = CreateDbContext();
        var request = await initialDbContext.SchoolBulkImportRequests.SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);
        request.ProcessingStarted();
        for (var lineNumber = 1; lineNumber <= 9; lineNumber++)
        {
            request.UpdateProgress(lineNumber, errorMessage: null);
        }

        request.ProcessingComplete();
        await initialDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = CreateHandler(CreateDbContext());

        // Act
        await handler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken);

        // Assert
        var verifyDbContext = CreateDbContext();
        var savedRequest = await verifyDbContext.SchoolBulkImportRequests.SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);
        Assert.Equal(9, savedRequest.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, savedRequest.Status);
        Assert.Empty(await verifyDbContext.Schools.ToListAsync(TestContext.Current.CancellationToken));
    }

    private static ProcessImportFileCommandHandler CreateHandler(ApplicationDbContext dbContext) =>
        new(dbContext, new LargeObjectReader(dbContext), new SchoolsImporter<ApplicationDbContext>(dbContext));

    private async Task<Guid> CreateImportRequestAsync(Stream content)
    {
        var dbContext = CreateDbContext();
        LargeObjectWriter writer = new(dbContext);
        var largeObjectId = await writer.StreamContentToNewLargeObjectAsync(content, TestContext.Current.CancellationToken);

        SchoolBulkImportRequest request = new()
        {
            ContentId = largeObjectId,
        };

        dbContext.SchoolBulkImportRequests.Add(request);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return request.Id;
    }

    private static MemoryStream CreateCsvStream(params string[] lines) =>
        new(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
}
