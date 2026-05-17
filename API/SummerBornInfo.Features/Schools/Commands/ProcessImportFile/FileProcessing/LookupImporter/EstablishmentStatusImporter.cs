namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;

internal sealed class EstablishmentStatusImporter<TContext>(TContext context)
    : LookupImporterBase<EstablishmentStatus, TContext>(context)
    where TContext : DbContext
{
    protected override Task<EstablishmentStatus?> FindByCodeAsync(
        TContext context, string code, CancellationToken cancellationToken)
    {
        return context.Set<EstablishmentStatus>()
               .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
    }

    protected override EstablishmentStatus Create(string code, string name)
    {
        return new() { Code = code, Name = name };
    }

    protected override void UpdateName(EstablishmentStatus entity, string name)
    {
        entity.Name = name;
    }
}
