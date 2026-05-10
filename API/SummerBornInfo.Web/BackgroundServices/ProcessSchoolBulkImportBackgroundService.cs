using SummerBornInfo.Domain.Events;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

namespace SummerBornInfo.Web.BackgroundServices;

public sealed class ProcessSchoolBulkImportBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProcessSchoolBulkImportBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromSeconds(1);

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
                logger.LogError(ex, "An error occurred while processing school bulk import messages.");
                await Task.Delay(EmptyQueueDelay, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessNextMessageAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var eventReader = scope.ServiceProvider.GetRequiredService<IEventReader>();
        var queuedEvent = await eventReader.ReadEventAsync<SchoolBulkImportUploaded>(EventQueue.SchoolBulkImport, 30, cancellationToken);

        if (queuedEvent is null)
        {
            return false;
        }

        try
        {
            var handler = scope.ServiceProvider.GetRequiredService<ProcessImportFileCommandHandler>();
            await handler.ExecuteAsync(new ProcessImportFileCommand(queuedEvent.Message.SchoolBulkImportRequestId), cancellationToken);
            await eventReader.DeleteEventAsync(EventQueue.SchoolBulkImport, queuedEvent.MessageId, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process school bulk import request {SchoolBulkImportRequestId}.", queuedEvent.Message.SchoolBulkImportRequestId);
            return false;
        }
    }
}
