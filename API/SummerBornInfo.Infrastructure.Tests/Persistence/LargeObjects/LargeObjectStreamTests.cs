using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence.LargeObjects;

public class LargeObjectStreamTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    private uint _largeObjectId;
    private byte[] _originalContent = [];

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        ApplicationDbContext dbContext = CreateDbContext();
        var largeObjectWriter = new LargeObjectWriter(dbContext);

        Stream sourceStream = ExampleImportFile.GetExampleImportFileContent();
        using var ms = new MemoryStream();
        await sourceStream.CopyToAsync(ms);
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
        LargeObjectStream stream = await CreateStream();
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCheckingCanSeek_ThenItShouldBeTrue()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.True(stream.CanSeek);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCheckingCanWrite_ThenItShouldBeFalse()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.False(stream.CanWrite);
    }

    // -------------------------------------------------------------------------
    // Length / GetLengthAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenAccessingLength_ThenItShouldMatchTheOriginalContentByteCount()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.Equal(_originalContent.Length, stream.Length);
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingGetLengthAsync_ThenItShouldMatchTheOriginalContentByteCount()
    {
        LargeObjectStream stream = await CreateStream();
        var length = await stream.GetLengthAsync(TestContext.Current.CancellationToken);
        Assert.Equal(_originalContent.Length, length);
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingGetLengthAsyncTwice_ThenBothCallsShouldReturnTheSameValue()
    {
        LargeObjectStream stream = await CreateStream();
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
        LargeObjectStream stream = await CreateStream();
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenANewStream_WhenSettingPosition_ThenPositionShouldReflectTheNewValue()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = 42;
        Assert.Equal(42L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamWithNonZeroPosition_WhenSettingPositionToZero_ThenPositionShouldBeZero()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = 10;
        stream.Position = 0;
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenANewStream_WhenSettingPositionToANegativeValue_ThenItShouldThrowArgumentOutOfRangeException()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = -1);
    }

    // -------------------------------------------------------------------------
    // Seek
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenANewStream_WhenSeekingFromBeginWithAnOffset_ThenPositionShouldEqualThatOffset()
    {
        LargeObjectStream stream = await CreateStream();
        long result = stream.Seek(10, SeekOrigin.Begin);
        Assert.Equal(10L, result);
        Assert.Equal(10L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamWithNonZeroPosition_WhenSeekingFromBeginWithZeroOffset_ThenPositionShouldBeZero()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = 50;
        long result = stream.Seek(0, SeekOrigin.Begin);
        Assert.Equal(0L, result);
        Assert.Equal(0L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamAtAKnownPosition_WhenSeekingFromCurrentWithAPositiveOffset_ThenPositionShouldAdvanceByThatOffset()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = 20;
        long result = stream.Seek(5, SeekOrigin.Current);
        Assert.Equal(25L, result);
        Assert.Equal(25L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenAStreamAtAKnownPosition_WhenSeekingFromCurrentWithANegativeOffset_ThenPositionShouldRetreatByThatOffset()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = 20;
        long result = stream.Seek(-5, SeekOrigin.Current);
        Assert.Equal(15L, result);
        Assert.Equal(15L, stream.Position);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenSeekingFromEndWithZeroOffset_ThenPositionShouldEqualTheLength()
    {
        LargeObjectStream stream = await CreateStream();
        long result = stream.Seek(0, SeekOrigin.End);
        Assert.Equal(_originalContent.Length, result);
        Assert.Equal(_originalContent.Length, stream.Position);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenSeekingFromEndWithANegativeOffset_ThenPositionShouldBeOffsetFromTheEnd()
    {
        LargeObjectStream stream = await CreateStream();
        long result = stream.Seek(-10, SeekOrigin.End);
        Assert.Equal(_originalContent.Length - 10, result);
        Assert.Equal(_originalContent.Length - 10, stream.Position);
    }

    [Fact]
    public async ValueTask GivenANewStream_WhenSeekingWithAnUnknownSeekOrigin_ThenItShouldThrowNotImplementedException()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.Throws<NotImplementedException>(() => stream.Seek(0, (SeekOrigin)99));
    }

    // -------------------------------------------------------------------------
    // Read (synchronous)
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenReadingSynchronouslyFromTheBeginning_ThenBytesReturnedShouldMatchTheLargeObjectAndPositionShouldAdvance()
    {
        LargeObjectStream stream = await CreateStream();
        byte[] buffer = new byte[16];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        Assert.True(bytesRead > 0);
        Assert.Equal(bytesRead, stream.Position);
        Assert.Equal(_originalContent[..bytesRead], buffer[..bytesRead]);
    }

    [Fact]
    public async ValueTask GivenAStreamPositionedAtTheEnd_WhenReadingSynchronously_ThenZeroBytesShouldBeReturned()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = _originalContent.Length;
        byte[] buffer = new byte[16];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        Assert.Equal(0, bytesRead);
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenReadingSynchronouslyWithABufferOffset_ThenBytesShouldBeWrittenToTheCorrectSlice()
    {
        LargeObjectStream stream = await CreateStream();
        byte[] buffer = new byte[32];
        const int offset = 8;
        const int count = 16;
        int bytesRead = stream.Read(buffer, offset, count);

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
        LargeObjectStream stream = await CreateStream();
        byte[] buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, TestContext.Current.CancellationToken);

        Assert.True(bytesRead > 0);
        Assert.Equal(bytesRead, stream.Position);
        Assert.Equal(_originalContent[..bytesRead], buffer[..bytesRead]);
    }

    [Fact]
    [SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Covers Testing Stream Implementation")]
    public async Task GivenAStreamPositionedAtTheEnd_WhenReadingAsynchronouslyViaTaskOverload_ThenZeroBytesShouldBeReturned()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = _originalContent.Length;
        byte[] buffer = new byte[16];
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, TestContext.Current.CancellationToken);
        Assert.Equal(0, bytesRead);
    }

    // -------------------------------------------------------------------------
    // ReadAsync (ValueTask / Memory overload)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingAsynchronouslyViaMemoryOverloadFromTheBeginning_ThenBytesReturnedShouldMatchTheLargeObjectAndPositionShouldAdvance()
    {
        LargeObjectStream stream = await CreateStream();
        Memory<byte> buffer = new byte[16].AsMemory();
        var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);

        Assert.True(bytesRead > 0);
        Assert.Equal(bytesRead, stream.Position);
        Assert.Equal(_originalContent[..bytesRead], buffer[..bytesRead].ToArray());
    }

    [Fact]
    public async Task GivenAStreamPositionedAtTheEnd_WhenReadingAsynchronouslyViaMemoryOverload_ThenZeroBytesShouldBeReturned()
    {
        LargeObjectStream stream = await CreateStream();
        stream.Position = _originalContent.Length;
        Memory<byte> buffer = new byte[16].AsMemory();
        var bytesRead = await stream.ReadAsync(buffer, TestContext.Current.CancellationToken);
        Assert.Equal(0, bytesRead);
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingAsynchronouslyViaMemoryOverloadInSequentialChunks_ThenAllChunksCombinedShouldMatchTheLargeObject()
    {
        LargeObjectStream stream = await CreateStream();
        var result = new MemoryStream();
        byte[] buffer = new byte[128];

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(), TestContext.Current.CancellationToken)) > 0)
        {
            result.Write(buffer, 0, bytesRead);
        }

        Assert.Equal(_originalContent, result.ToArray());
    }

    // -------------------------------------------------------------------------
    // Full end-to-end read (matches original content)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingTheStreamEndToEnd_ThenTheResultShouldMatchTheLargeObject()
    {
        LargeObjectStream stream = await CreateStream();
        using var result = new MemoryStream();
        await stream.CopyToAsync(result, TestContext.Current.CancellationToken);
        Assert.Equal(_originalContent, result.ToArray());
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenReadingEndToEndThenSeekingToBeginAndReadingAgain_ThenBothReadsShouldProduceTheSameResult()
    {
        LargeObjectStream stream = await CreateStream();

        using var first = new MemoryStream();
        await stream.CopyToAsync(first, TestContext.Current.CancellationToken);

        stream.Seek(0, SeekOrigin.Begin);

        using var second = new MemoryStream();
        await stream.CopyToAsync(second, TestContext.Current.CancellationToken);

        Assert.Equal(first.ToArray(), second.ToArray());
    }

    // -------------------------------------------------------------------------
    // Flush (no-op — just must not throw)
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenFlushingTheStream_ThenItShouldNotThrow()
    {
        LargeObjectStream stream = await CreateStream();
        var ex = Record.Exception(() => stream.Flush());
        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // Unsupported write operations
    // -------------------------------------------------------------------------

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCallingSetLength_ThenItShouldThrowNotSupportedException()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.Throws<NotSupportedException>(() => stream.SetLength(100));
    }

    [Fact]
    public async ValueTask GivenALargeObjectExists_WhenCallingSynchronousWrite_ThenItShouldThrowNotSupportedException()
    {
        LargeObjectStream stream = await CreateStream();
        Assert.Throws<NotSupportedException>(() => stream.Write([1, 2, 3], 0, 3));
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingWriteAsyncViaTaskOverload_ThenItShouldThrowNotSupportedException()
    {
        LargeObjectStream stream = await CreateStream();
        await Assert.ThrowsAsync<NotSupportedException>(
            () => stream.WriteAsync([1, 2, 3], 0, 3, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GivenALargeObjectExists_WhenCallingWriteAsyncViaMemoryOverload_ThenItShouldThrowNotSupportedException()
    {
        LargeObjectStream stream = await CreateStream();
        await Assert.ThrowsAsync<NotSupportedException>(
            async () => await stream.WriteAsync(new ReadOnlyMemory<byte>([1, 2, 3]), TestContext.Current.CancellationToken));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async ValueTask<LargeObjectStream> CreateStream()
    {
        ApplicationDbContext dbContext = CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);
        return new LargeObjectStream(dbContext.GetNpgsqlConnection(), _largeObjectId);
    }
}