using SummerBornInfo.Domain.Events;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile;
using System.Diagnostics;

namespace SummerBornInfo.Web.BackgroundServices;

public sealed class ProcessSchoolBulkImportBackgroundService(
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
                logger.LogError(ex, "An error occurred while processing school bulk import messages.");
                await Task.Delay(emptyQueueDelay, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessNextMessageAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var eventReader = scope.ServiceProvider.GetRequiredService<IEventReader>();
        var eventAcknowledger = scope.ServiceProvider.GetRequiredService<IEventAcknowledger>();
        var queuedEvent = await eventReader.ReadEventAsync<SchoolBulkImportUploaded>(EventQueue.SchoolBulkImport, messageReadTimeoutSeconds, cancellationToken);

        if (queuedEvent is null)
        {
            return false;
        }

        using var activity = SchoolBulkImportTelemetry.ActivitySource.StartActivity(SchoolBulkImportTelemetry.ActivityName, ActivityKind.Consumer);
        activity?.SetTag("schoolBulkImport.request_id", queuedEvent.Message.SchoolBulkImportRequestId);
        activity?.SetTag("messaging.message.id", queuedEvent.MessageId);
        activity?.SetTag("messaging.destination", EventQueue.SchoolBulkImport.Name);

        try
        {
            var handler = scope.ServiceProvider.GetRequiredService<ProcessImportFileCommandHandler>();
            await handler.ExecuteAsync(new ProcessImportFileCommand(queuedEvent.Message.SchoolBulkImportRequestId), cancellationToken);
            activity?.SetTag("schoolBulkImport.outcome", "processed");
            await eventAcknowledger.DeleteEventAsync(EventQueue.SchoolBulkImport, queuedEvent.MessageId, cancellationToken);
            activity?.SetTag("schoolBulkImport.outcome", "acknowledged");
            activity?.SetStatus(ActivityStatusCode.Ok);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetTag("schoolBulkImport.outcome", "failed");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            logger.LogError(ex, "Failed to process school bulk import request {SchoolBulkImportRequestId}.", queuedEvent.Message.SchoolBulkImportRequestId);
            return false;
        }
    }
}
