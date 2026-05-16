using SummerBornInfo.Domain.Events;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

namespace SummerBornInfo.Web.BackgroundServices;

public sealed partial class ProcessSchoolBulkImportBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    Microsoft.Extensions.Options.IOptions<SchoolBulkImportWorkerOptions> options,
    ILogger<ProcessSchoolBulkImportBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan emptyQueueDelay = TimeSpan.FromSeconds(Math.Max(0, options.Value.EmptyQueueDelaySeconds));
    private readonly int messageReadTimeoutSeconds = Math.Max(1, options.Value.MessageReadTimeoutSeconds);

    internal TimeSpan EmptyQueueDelay => emptyQueueDelay;
    internal int MessageReadTimeoutSeconds => messageReadTimeoutSeconds;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedMessage = await ProcessNextMessageAsync(stoppingToken);
                if (!processedMessage)
                {
                    await Task.Delay(emptyQueueDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogUnhandledProcessingError(logger, ex);
                await Task.Delay(emptyQueueDelay, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessNextMessageAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var eventReader = scope.ServiceProvider.GetRequiredService<IEventReader>();
        var eventAcknowledger = scope.ServiceProvider.GetRequiredService<IEventAcknowledger>();
        var queuedEvent = await eventReader.ReadEventAsync<SchoolBulkImportUploaded>(EventQueue.SchoolBulkImport, messageReadTimeoutSeconds, cancellationToken);

        if (queuedEvent is null)
        {
            return false;
        }

        try
        {
            var handler = scope.ServiceProvider.GetRequiredService<ProcessImportFileCommandHandler>();
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
}
