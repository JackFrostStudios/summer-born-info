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
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.ExecuteAsync(command, TestContext.Current.CancellationToken));
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

        _ = dbContext.SchoolBulkImportRequests.Add(schoolBulkImportRequest);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = CreateHandler(CreateDbContext());
        ProcessImportFileCommand command = new(schoolBulkImportRequest.Id);

        // Act / Assert
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.ExecuteAsync(command, TestContext.Current.CancellationToken));
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
        // Arrange
        await using var invalidCsv = CreateCsvStream(
            "\"URN\",\"EstablishmentNumber\",\"EstablishmentName\",\"LA (code)\",\"LA (name)\",\"TypeOfEstablishment (code)\",\"TypeOfEstablishment (name)\",\"EstablishmentTypeGroup (code)\",\"EstablishmentTypeGroup (name)\",\"EstablishmentStatus (code)\",\"EstablishmentStatus (name)\",\"PhaseOfEducation (code)\",\"PhaseOfEducation (name)\",\"OpenDate\",\"CloseDate\",\"UKPRN\",\"Street\",\"Locality\",\"Address3\",\"Town\",\"County (name)\",\"Postcode\",\"Easting\",\"Northing\"",
            "\"100000\",\"3614\",\"The Aldgate School\",\"201\",\"City of London\",\"02\",\"Voluntary aided school\",\"4\",\"Local authority maintained schools\",\"1\",\"Open\",\"2\",\"Primary\",\"\",\"\",\"10079319\",\"St James's Passage\",\"Duke's Place\",\"\",\"London\",\"\",\"EC3A 5DE\",\"533523\",\"181201\"",
            "\"INVALID\",\"1045\",\"Broken School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\",\"528515\",\"184869\"",
            "\"100004\",\"1045\",\"Sherborne Nursery School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\",\"528515\",\"184869\"");
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
        _ = Assert.Single(request.Failures);
        Assert.Equal(3, request.Failures[0].LineNumber);
        Assert.NotNull(request.Failures[0].ErrorMessage);

        var schools = await verifyDbContext.Schools
            .OrderBy(x => x.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([100000, 100004], [.. schools.Select(x => x.URN)]);
    }

    [Fact]
    public async Task GivenCompletedImportRequest_WhenExecuted_ThenRequestIsNotProcessedAgain()
    {
        // Arrange
        var requestId = await CreateImportRequestAsync(ExampleImportFile.GetExampleImportFileContent());
        var initialDbContext = CreateDbContext();
        var request = await initialDbContext.SchoolBulkImportRequests.SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);
        _ = request.ProcessingStarted();
        for (var lineNumber = 1; lineNumber <= 9; lineNumber++)
        {
            request.UpdateProgress(lineNumber, errorMessage: null);
        }

        request.ProcessingComplete();
        _ = await initialDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task GivenUnexpectedImporterFailure_WhenExecuted_ThenFailureIsMarkedAndGenericExceptionIsThrown()
    {
        // Arrange
        var requestId = await CreateImportRequestAsync(ExampleImportFile.GetExampleImportFileContent());
        var dbContext = CreateDbContext();
        var handler = new ProcessImportFileCommandHandler(
            dbContext,
            new LargeObjectReader(dbContext),
            new ThrowingSchoolsImporter(_ => throw new InvalidOperationException("SQL details should not leak.")));

        // Act
        var exception = await Assert.ThrowsAsync<SchoolBulkImportProcessingException>(
            () => handler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal("The import file could not be processed. Please try again.", exception.Message);

        var verifyDbContext = CreateDbContext();
        var request = await verifyDbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);

        Assert.Equal(SchoolBulkImportStatus.Failed, request.Status);
        Assert.Equal(0, request.LinesProcessed);
        Assert.Empty(request.Failures);
    }

    [Fact]
    public async Task GivenPreviouslyFailedRequestWithPartialProgress_WhenExecutedAgain_ThenProcessingResumesFromLastProcessedRow()
    {
        // Arrange
        var requestId = await CreateImportRequestAsync(ExampleImportFile.GetExampleImportFileContent());
        var firstRunDbContext = CreateDbContext();
        var firstRunHandler = new ProcessImportFileCommandHandler(
            firstRunDbContext,
            new LargeObjectReader(firstRunDbContext),
            new ThrowingSchoolsImporter(async cancellationToken =>
            {
                var importerDbContext = CreateDbContext();
                SchoolsImporter<ApplicationDbContext> importer = new(
                    importerDbContext,
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<SchoolsImporter<ApplicationDbContext>>.Instance);
                await using var csvStream = ExampleImportFile.GetExampleImportFileContent();
                var firstResult = await importer.ImportAsync(requestId, csvStream, cancellationToken).FirstAsync(cancellationToken);
                return firstResult;
            }));

        _ = await Assert.ThrowsAsync<SchoolBulkImportProcessingException>(
            () => firstRunHandler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken));

        var retryHandler = CreateHandler(CreateDbContext());

        // Act
        await retryHandler.ExecuteAsync(new ProcessImportFileCommand(requestId), TestContext.Current.CancellationToken);

        // Assert
        var verifyDbContext = CreateDbContext();
        var request = await verifyDbContext.SchoolBulkImportRequests
            .Include(x => x.Failures)
            .SingleAsync(x => x.Id == requestId, TestContext.Current.CancellationToken);

        Assert.Equal(2, request.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Completed, request.Status);
        Assert.Empty(request.Failures);

        var schools = await verifyDbContext.Schools
            .OrderBy(x => x.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([100000, 100004], [.. schools.Select(x => x.URN)]);
    }

    private static ProcessImportFileCommandHandler CreateHandler(ApplicationDbContext dbContext)
    {
        return new(
            dbContext,
            new LargeObjectReader(dbContext),
            new SchoolsImporter<ApplicationDbContext>(
                dbContext,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<SchoolsImporter<ApplicationDbContext>>.Instance));
    }

    private async Task<Guid> CreateImportRequestAsync(Stream content)
    {
        var dbContext = CreateDbContext();
        LargeObjectWriter writer = new(dbContext);
        var largeObjectId = await writer.StreamContentToNewLargeObjectAsync(content, TestContext.Current.CancellationToken);

        SchoolBulkImportRequest request = new()
        {
            ContentId = largeObjectId,
        };

        _ = dbContext.SchoolBulkImportRequests.Add(request);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return request.Id;
    }

    private static MemoryStream CreateCsvStream(params string[] lines)
    {
        return new(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
    }

    private sealed class ThrowingSchoolsImporter(Func<CancellationToken, Task<SchoolImportResult>> firstResultFactory) : ISchoolsImporter
    {
        public async IAsyncEnumerable<SchoolImportResult> ImportAsync(
            Guid schoolBulkImportRequestId,
            Stream csvStream,
            int processedRowsToSkip = 0,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (processedRowsToSkip > 0)
            {
                yield break;
            }

            yield return await firstResultFactory(cancellationToken);
            throw new InvalidOperationException("SQL details should not leak.");
        }
    }
}
