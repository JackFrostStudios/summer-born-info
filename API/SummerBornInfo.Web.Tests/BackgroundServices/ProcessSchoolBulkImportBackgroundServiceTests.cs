namespace SummerBornInfo.Web.Tests.BackgroundServices;

public sealed class ProcessSchoolBulkImportBackgroundServiceTests
{
    [Fact]
    public async Task GivenMessageReadCountExceedsMaxRetry_WhenProcessingNextMessage_ThenMessageIsAcknowledgedWithoutReprocessing()
    {
        // Arrange
        var requestId = Guid.CreateVersion7();
        FakeEventReader eventReader = new(new QueuedEvent<SchoolBulkImportUploaded>(
            MessageId: 42,
            Message: new SchoolBulkImportUploaded { SchoolBulkImportRequestId = requestId },
            ReadCount: 4));
        FakeEventAcknowledger eventAcknowledger = new();
        FakeProcessImportFileCommandHandler handler = new();
        var service = CreateService(eventReader, eventAcknowledger, handler, maxRetryCount: 3);

        // Act
        var processed = await service.ProcessNextMessageAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(processed);
        Assert.Equal(0, handler.ExecutionCount);
        Assert.Equal(1, eventAcknowledger.DeleteCount);
        Assert.Equal(42, eventAcknowledger.LastDeletedMessageId);
    }

    [Fact]
    public async Task GivenMessageReadCountWithinMaxRetry_WhenProcessingNextMessage_ThenMessageIsProcessed()
    {
        // Arrange
        var requestId = Guid.CreateVersion7();
        FakeEventReader eventReader = new(new QueuedEvent<SchoolBulkImportUploaded>(
            MessageId: 43,
            Message: new SchoolBulkImportUploaded { SchoolBulkImportRequestId = requestId },
            ReadCount: 3));
        FakeEventAcknowledger eventAcknowledger = new();
        FakeProcessImportFileCommandHandler handler = new();
        var service = CreateService(eventReader, eventAcknowledger, handler, maxRetryCount: 3);

        // Act
        var processed = await service.ProcessNextMessageAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(processed);
        Assert.Equal(1, handler.ExecutionCount);
        Assert.Equal(1, eventAcknowledger.DeleteCount);
        Assert.Equal(requestId, handler.LastRequestId);
    }

    private static ProcessSchoolBulkImportBackgroundService CreateService(
        IEventReader eventReader,
        IEventAcknowledger eventAcknowledger,
        IProcessImportFileCommandHandler handler,
        int maxRetryCount)
    {
        ServiceCollection services = [];
        _ = services.AddSingleton(eventReader);
        _ = services.AddSingleton(eventAcknowledger);
        _ = services.AddSingleton(handler);

        var rootProvider = services.BuildServiceProvider();
        var scopeFactory = rootProvider.GetRequiredService<IServiceScopeFactory>();
        var options = Options.Create(new SchoolBulkImportWorkerOptions
        {
            EmptyQueueDelaySeconds = 1,
            MessageReadTimeoutSeconds = 30,
            MaxRetryCount = maxRetryCount,
        });
        var logger = LoggerFactory.Create(static _ => { }).CreateLogger<ProcessSchoolBulkImportBackgroundService>();

        return new ProcessSchoolBulkImportBackgroundService(scopeFactory, options, logger);
    }

    private sealed class FakeEventReader(QueuedEvent<SchoolBulkImportUploaded>? queuedEvent) : IEventReader
    {
        public Task<QueuedEvent<T>?> ReadEventAsync<T>(IEventQueue queue, int messageReadTimeoutSeconds, CancellationToken cancellationToken)
            where T : class
        {
            return Task.FromResult(queuedEvent as QueuedEvent<T>);
        }
    }

    private sealed class FakeEventAcknowledger : IEventAcknowledger
    {
        public int DeleteCount { get; private set; }
        public long? LastDeletedMessageId { get; private set; }

        public Task DeleteEventAsync(IEventQueue queue, long messageId, CancellationToken cancellationToken)
        {
            DeleteCount++;
            LastDeletedMessageId = messageId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProcessImportFileCommandHandler : IProcessImportFileCommandHandler
    {
        public int ExecutionCount { get; private set; }
        public Guid? LastRequestId { get; private set; }

        public Task ExecuteAsync(ProcessImportFileCommand command, CancellationToken cancellationToken)
        {
            ExecutionCount++;
            LastRequestId = command.SchoolBulkImportRequestId;
            return Task.CompletedTask;
        }
    }
}
