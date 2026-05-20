namespace SummerBornInfo.Web.BackgroundServices;

public sealed partial class ProcessSchoolBulkImportBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    Microsoft.Extensions.Options.IOptions<SchoolBulkImportWorkerOptions> options,
    ILogger<ProcessSchoolBulkImportBackgroundService> logger) : BackgroundService
{
    internal TimeSpan EmptyQueueDelay { get; } = TimeSpan.FromSeconds(Math.Max(0, options.Value.EmptyQueueDelaySeconds));
    internal int MessageReadTimeoutSeconds { get; } = Math.Max(1, options.Value.MessageReadTimeoutSeconds);
    internal int MaxRetryCount { get; } = Math.Max(0, options.Value.MaxRetryCount);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedMessage = await ProcessNextMessageAsync(stoppingToken);
                if (!processedMessage)
                {
                    await Task.Delay(EmptyQueueDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogUnhandledProcessingError(logger, ex);
                await Task.Delay(EmptyQueueDelay, stoppingToken);
            }
        }
    }

    internal async Task<bool> ProcessNextMessageAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var eventReader = scope.ServiceProvider.GetRequiredService<IEventReader>();
        var eventAcknowledger = scope.ServiceProvider.GetRequiredService<IEventAcknowledger>();
        var queuedEvent = await eventReader.ReadEventAsync<SchoolBulkImportUploaded>(EventQueue.SchoolBulkImport, MessageReadTimeoutSeconds, cancellationToken);

        if (queuedEvent is null)
        {
            return false;
        }

        if (queuedEvent.ReadCount > MaxRetryCount)
        {
            LogPoisonMessageAcknowledged(logger, queuedEvent.Message.SchoolBulkImportRequestId, queuedEvent.ReadCount, MaxRetryCount);
            await eventAcknowledger.DeleteEventAsync(EventQueue.SchoolBulkImport, queuedEvent.MessageId, cancellationToken);
            return true;
        }

        try
        {
            var handler = scope.ServiceProvider.GetRequiredService<IProcessImportFileCommandHandler>();
            await handler.ExecuteAsync(new ProcessImportFileCommand(queuedEvent.Message.SchoolBulkImportRequestId), cancellationToken);
            await eventAcknowledger.DeleteEventAsync(EventQueue.SchoolBulkImport, queuedEvent.MessageId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            LogFailedToProcessRequest(logger, queuedEvent.Message.SchoolBulkImportRequestId, ex);
            return false;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occurred while processing school bulk import messages.")]
    private static partial void LogUnhandledProcessingError(ILogger logger, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to process school bulk import request {SchoolBulkImportRequestId}.")]
    private static partial void LogFailedToProcessRequest(
        ILogger logger,
        Guid schoolBulkImportRequestId,
        Exception exception);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Acknowledged poison school bulk import message for request {SchoolBulkImportRequestId} after {ReadCount} reads exceeded the configured max retry count of {MaxRetryCount}.")]
    private static partial void LogPoisonMessageAcknowledged(
        ILogger logger,
        Guid schoolBulkImportRequestId,
        long readCount,
        int maxRetryCount);
}
