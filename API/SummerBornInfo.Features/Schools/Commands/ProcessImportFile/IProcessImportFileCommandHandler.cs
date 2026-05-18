namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile;

public interface IProcessImportFileCommandHandler
{
    Task ExecuteAsync(ProcessImportFileCommand command, CancellationToken cancellationToken);
}
