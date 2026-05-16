using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence.LargeObjects;

public class LargeObjectReaderTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
: IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenReadingToStream_ThenReadingStreamEndToEndWillReturnTheLargeObject()
    {
        // Arrange
        var writerDbContext = CreateDbContext();
        LargeObjectWriter largeObjectWriter = new(writerDbContext);
        var largeObjectStream = ExampleImportFile.GetExampleImportFileContent();
        var largeObjectId = await largeObjectWriter.StreamContentToNewLargeObjectAsync(largeObjectStream, TestContext.Current.CancellationToken);

        var readerDbContext = CreateDbContext();
        LargeObjectReader largeObjectReader = new(readerDbContext);

        // Act
        var result = await largeObjectReader.ReadLargeObjectAsStreamAsync(largeObjectId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        await AssertOriginalLargeObjectMatchesResultAsync(largeObjectStream, result, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async ValueTask GivenConnectionIsOpen_WhenReadingToStream_ThenNoExceptionIsThrown()
    {
        // Arrange
        var readerDbContext = CreateDbContext();
        await readerDbContext.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        LargeObjectReader largeObjectReader = new(readerDbContext);

        // Act
        var result = await Record.ExceptionAsync(async () => await largeObjectReader.ReadLargeObjectAsStreamAsync(999, TestContext.Current.CancellationToken));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async ValueTask GivenALargeObjectDoesNotExists_WhenReadingToStream_ThenNullIsReturned()
    {
        // Arrange
        var readerDbContext = CreateDbContext();
        LargeObjectReader largeObjectReader = new(readerDbContext);

        // Act
        var result = await largeObjectReader.ReadLargeObjectAsStreamAsync(999, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
    }

    private static async ValueTask AssertOriginalLargeObjectMatchesResultAsync(Stream expectedObject, Stream actualObject, CancellationToken cancellationToken)
    {
        var expectedData = await GetDataFromStreamAsync(expectedObject, cancellationToken);
        var actualData = await GetDataFromStreamAsync(actualObject, cancellationToken);
        Assert.Equal(expectedData.Length, actualData.Length);

        Assert.Equal(expectedData, actualData);
    }

    private static async ValueTask<byte[]> GetDataFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        stream.Seek(0, SeekOrigin.Begin);
        byte[] data;
        await using MemoryStream expectedMemoryStream = new();
        await stream.CopyToAsync(expectedMemoryStream, cancellationToken);
        data = expectedMemoryStream.ToArray();
        return data;
    }
}
