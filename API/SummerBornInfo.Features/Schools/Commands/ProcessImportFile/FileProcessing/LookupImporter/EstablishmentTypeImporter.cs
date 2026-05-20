namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;

internal sealed class EstablishmentTypeImporter<TContext>(TContext context)
    : LookupImporterBase<EstablishmentType, TContext>(context)
    where TContext : DbContext
{
    protected override Task<EstablishmentType?> FindByCodeAsync(
        TContext context, string code, CancellationToken cancellationToken)
    {
        return context.Set<EstablishmentType>()
               .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);
    }

    protected override EstablishmentType Create(string code, string name)
    {
        return new() { Code = code, Name = name };
    }

    protected override void UpdateName(EstablishmentType entity, string name)
    {
        entity.Name = name;
    }
}
