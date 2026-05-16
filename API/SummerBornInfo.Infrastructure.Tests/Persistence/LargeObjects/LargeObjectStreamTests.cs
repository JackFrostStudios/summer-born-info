namespace SummerBornInfo.Infrastructure.Tests.Persistence.LargeObjects;

public class LargeObjectStreamTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    private uint _largeObjectId;
    private byte[] _originalContent = [];

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        var dbContext = CreateDbContext();
        LargeObjectWriter largeObjectWriter = new(dbContext);

        var sourceStream = ExampleImportFile.GetExampleImportFileContent();
        await using MemoryStream ms = new();
        await sourceStream.CopyToAsync(ms, TestContext.Current.CancellationToken);
        _originalContent = ms.ToArray();

        ms.Position = 0;
        _largeObjectId = await largeObjectWriter.StreamContentToNewLargeObjectAsync(ms, TestContext.Current.CancellationToken);
    }

    // -------------------------------------------------------------------------
    // Initializing
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCreatingStream_ThenNoExceptionIsThrown()
    {
        var ex = await Record.ExceptionAsync(async () => await CreateStream());
        Assert.Null(ex);
    }

    [Fact]
    public async ValueTask GivenALargeObjectDoesNotExists_WhenCreatingStream_ThenNoExceptionIsThrown()
    {
        var ex = Record.Exception(() => new LargeObjectStream(CreateDbContext().GetNpgsqlConnection(), _largeObjectId + 1));
        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // CanRead / CanSeek / CanWrite
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCheckingCanRead_ThenItShouldBeTrue()
    {
        var stream = await CreateStream();
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCheckingCanSeek_ThenItShouldBeTrue()
    {
        var stream = await CreateStream();
        Assert.True(stream.CanSeek);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCheckingCanWrite_ThenItShouldBeFalse()
    {
        var stream = await CreateStream();
        Assert.False(stream.CanWrite);
    }

    // -------------------------------------------------------------------------
    // Length / GetLengthAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenAccessingLength_ThenItShouldMatchTheOriginalContentByteCount()
    {
        var stream = await CreateStream();
        Assert.Equal(_originalContent.Length, stream.Length);
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingGetLengthAsync_ThenItShouldMatchTheOriginalContentByteCount()
    {
        var stream = await CreateStream();
        var length = await stream.GetLengthAsync(TestContext.Current.CancellationToken);
        Assert.Equal(_originalContent.Length, length);
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingGetLengthAsyncTwice_ThenBothCallsShouldReturnTheSameValue()
    {
        var stream = await CreateStream();
        var first = await stream.GetLengthAsync(TestContext.Current.CancellationToken);
        var second = await stream.GetLengthAsync(TestContext.Current.CancellationToken);
        Assert.Equal(first, second);
    }

    // -------------------------------------------------------------------------
    // Position
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenANewStream_WhenAccessingPosition_ThenItShouldBeZero()
    {
        var stream = await CreateStream();
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenANewStream_WhenSettingPosition_ThenPositionShouldReflectTheNewValue()
    {
        var stream = await CreateStream();
        stream.Position = 42;
        Assert.Equal(42L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamWithNonZeroPosition_WhenSettingPositionToZero_ThenPositionShouldBeZero()
    {
        var stream = await CreateStream();
        stream.Position = 10;
        stream.Position = 0;
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenANewStream_WhenSettingPositionToANegativeValue_ThenItShouldThrowArgumentOutOfRangeException()
    {
        var stream = await CreateStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = -1);
    }

    // -------------------------------------------------------------------------
    // Seek
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenANewStream_WhenSeekingFromBeginWithAnOffset_ThenPositionShouldEqualThatOffset()
    {
        var stream = await CreateStream();
        var result = stream.Seek(10, SeekOrigin.Begin);
        Assert.Equal(10L, result);
        Assert.Equal(10L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamWithNonZeroPosition_WhenSeekingFromBeginWithZeroOffset_ThenPositionShouldBeZero()
    {
        var stream = await CreateStream();
        stream.Position = 50;
        var result = stream.Seek(0, SeekOrigin.Begin);
        Assert.Equal(0L, result);
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamAtAKnownPosition_WhenSeekingFromCurrentWithAPositiveOffset_ThenPositionShouldAdvanceByThatOffset()
    {
        var stream = await CreateStream();
        stream.Position = 20;
        var result = stream.Seek(5, SeekOrigin.Current);
        Assert.Equal(25L, result);
        Assert.Equal(25L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamAtAKnownPosition_WhenSeekingFromCurrentWithANegativeOffset_ThenPositionShouldRetreatByThatOffset()
    {
        var stream = await CreateStream();
        stream.Position = 20;
        var result = stream.Seek(-5, SeekOrigin.Current);
        Assert.Equal(15L, result);
        Assert.Equal(15L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenSeekingFromEndWithZeroOffset_ThenPositionShouldEqualTheLength()
    {
        var stream = await CreateStream();
        var result = stream.Seek(0, SeekOrigin.End);
        Assert.Equal(_originalContent.Length, result);
        Assert.Equal(_originalContent.Length, stream.Position);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenSeekingFromEndWithANegativeOffset_ThenPositionShouldBeOffsetFromTheEnd()
    {
        var stream = await CreateStream();
        var result = stream.Seek(-10, SeekOrigin.End);
        Assert.Equal(_originalContent.Length - 10, result);
        Assert.Equal(_originalContent.Length - 10, stream.Position);
    }

    [Fact]
    public async ValueTask GivenANewStream_WhenSeekingWithAnUnknownSeekOrigin_ThenItShouldThrowNotSupportedException()
    {
        var stream = await CreateStream();
        Assert.Throws<NotSupportedException>(() => stream.Seek(0, (SeekOrigin)99));
    }

    // -------------------------------------------------------------------------
    // Read (synchronous)
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenReadingSynchronouslyFromTheBeginning_ThenBytesReturnedShouldMatchTheLargeObjectAndPositionShouldAdvance()
    {
        var stream = await CreateStream();
        var buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);

        Assert.True(bytesRead > 0);
        Assert.Equal(bytesRead, stream.Position);
        Assert.Equal(_originalContent[..bytesRead], buffer[..bytesRead]);
    }

    [Fact]
    public async ValueTask GivenAStreamPositionedAtTheEnd_WhenReadingSynchronously_ThenZeroBytesShouldBeReturned()
    {
        var stream = await CreateStream();
        stream.Position = _originalContent.Length;
        var buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);
        Assert.Equal(0, bytesRead);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenReadingSynchronouslyWithABufferOffset_ThenBytesShouldBeWrittenToTheCorrectSlice()
    {
        var stream = await CreateStream();
        var buffer = new byte[32];
        const int offset = 8;
        const int count = 16;
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, count), TestContext.Current.CancellationToken);

        Assert.True(bytesRead > 0);
        // Bytes before offset must remain zero
        Assert.All(buffer[..offset], b => Assert.Equal(0, b));
        // Bytes after offset+bytesRead must remain zero
        Assert.All(buffer[(offset + bytesRead)..], b => Assert.Equal(0, b));
    }

    // -------------------------------------------------------------------------
    // ReadAsync (Task overload)
    // -------------------------------------------------------------------------

    [Fact]
    [SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Covers Testing Stream Implementation")]
    public async Task GivenALargeObjectExists_WhenReadingAsynchronouslyViaTaskOverloadFromTheBeginning_ThenBytesReturnedShouldMatchTheLargeObjectAndPositionShouldAdvance()
    {
        var stream = await CreateStream();
        var buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, TestContext.Current.CancellationToken);

        Assert.True(bytesRead > 0);
        Assert.Equal(bytesRead, stream.Position);
        Assert.Equal(_originalContent[..bytesRead], buffer[..bytesRead]);
    }

    [Fact]
    [SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Covers Testing Stream Implementation")]
    public async Task GivenAStreamPositionedAtTheEnd_WhenReadingAsynchronouslyViaTaskOverload_ThenZeroBytesShouldBeReturned()
    {
        var stream = await CreateStream();
        stream.Position = _originalContent.Length;
        var buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, TestContext.Current.CancellationToken);
        Assert.Equal(0, bytesRead);
    }

    // -------------------------------------------------------------------------
    // ReadAsync (ValueTask / Memory overload)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingAsynchronouslyViaMemoryOverloadFromTheBeginning_ThenBytesReturnedShouldMatchTheLargeObjectAndPositionShouldAdvance()
    {
        var stream = await CreateStream();
        var buffer = new byte[16].AsMemory();
        var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);

        Assert.True(bytesRead > 0);
        Assert.Equal(bytesRead, stream.Position);
        Assert.Equal(_originalContent[..bytesRead], buffer[..bytesRead].ToArray());
    }

    [Fact]
    public async Task GivenAStreamPositionedAtTheEnd_WhenReadingAsynchronouslyViaMemoryOverload_ThenZeroBytesShouldBeReturned()
    {
        var stream = await CreateStream();
        stream.Position = _originalContent.Length;
        var buffer = new byte[16].AsMemory();
        var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);
        Assert.Equal(0, bytesRead);
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingAsynchronouslyViaMemoryOverloadInSequentialChunks_ThenAllChunksCombinedShouldMatchTheLargeObject()
    {
        var stream = await CreateStream();
        MemoryStream result = new();
        var buffer = new byte[128];

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(), TestContext.Current.CancellationToken)) > 0)
        {
            await result.WriteAsync(buffer.AsMemory(0, bytesRead), TestContext.Current.CancellationToken);
        }

        Assert.Equal(_originalContent, result.ToArray());
    }

    // -------------------------------------------------------------------------
    // Full end-to-end read (matches original content)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingTheStreamEndToEnd_ThenTheResultShouldMatchTheLargeObject()
    {
        var stream = await CreateStream();
        await using MemoryStream result = new();
        await stream.CopyToAsync(result, TestContext.Current.CancellationToken);
        Assert.Equal(_originalContent, result.ToArray());
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingEndToEndThenSeekingToBeginAndReadingAgain_ThenBothReadsShouldProduceTheSameResult()
    {
        var stream = await CreateStream();
        await using MemoryStream first = new();
        await stream.CopyToAsync(first, TestContext.Current.CancellationToken);

        stream.Seek(0, SeekOrigin.Begin);
        await using MemoryStream second = new();
        await stream.CopyToAsync(second, TestContext.Current.CancellationToken);

        Assert.Equal(first.ToArray(), second.ToArray());
    }

    // -------------------------------------------------------------------------
    // Flush (no-op — just must not throw)
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenFlushingTheStream_ThenItShouldNotThrow()
    {
        var stream = await CreateStream();
        var ex = Record.Exception(stream.Flush);
        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // Unsupported write operations
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCallingSetLength_ThenItShouldThrowNotSupportedException()
    {
        var stream = await CreateStream();
        Assert.Throws<NotSupportedException>(() => stream.SetLength(100));
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCallingSynchronousWrite_ThenItShouldThrowNotSupportedException()
    {
        var stream = await CreateStream();
        Assert.Throws<NotSupportedException>(() => stream.Write([1, 2, 3], 0, 3));
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingWriteAsyncViaTaskOverload_ThenItShouldThrowNotSupportedException()
    {
        var stream = await CreateStream();
        await Assert.ThrowsAsync<NotSupportedException>(
            () => stream.WriteAsync([1, 2, 3], 0, 3, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingWriteAsyncViaMemoryOverload_ThenItShouldThrowNotSupportedException()
    {
        var stream = await CreateStream();
        await Assert.ThrowsAsync<NotSupportedException>(
            async () => await stream.WriteAsync(new ReadOnlyMemory<byte>([1, 2, 3]), TestContext.Current.CancellationToken));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async ValueTask<LargeObjectStream> CreateStream()
    {
        var dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        return new LargeObjectStream(dbContext.GetNpgsqlConnection(), _largeObjectId);
    }
}
