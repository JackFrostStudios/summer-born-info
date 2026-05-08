namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;

internal sealed class LocalAuthorityImporter<TContext>(TContext context)
    : LookupImporterBase<LocalAuthority, TContext>(context)
    where TContext : DbContext
{
    protected override Task<LocalAuthority?> FindByCodeAsync(
        TContext context, string code, CancellationToken cancellationToken) =>
        context.Set<LocalAuthority>()
               .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);

    protected override LocalAuthority Create(string code, string name) =>
        new() { Code = code, Name = name };

    protected override void UpdateName(LocalAuthority entity, string name) =>
        entity.Name = name;
}
