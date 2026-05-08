namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;

internal sealed class EstablishmentGroupImporter<TContext>(TContext context)
    : LookupImporterBase<EstablishmentGroup, TContext>(context)
    where TContext : DbContext
{
    protected override Task<EstablishmentGroup?> FindByCodeAsync(
        TContext context, string code, CancellationToken cancellationToken) =>
        context.Set<EstablishmentGroup>()
               .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);

    protected override EstablishmentGroup Create(string code, string name) =>
        new() { Code = code, Name = name };

    protected override void UpdateName(EstablishmentGroup entity, string name) =>
        entity.Name = name;
}
