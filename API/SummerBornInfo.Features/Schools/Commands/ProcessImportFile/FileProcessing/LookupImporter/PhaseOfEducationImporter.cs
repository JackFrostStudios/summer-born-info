namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;

internal sealed class PhaseOfEducationImporter<TContext>(TContext context)
    : LookupImporterBase<PhaseOfEducation, TContext>(context)
    where TContext : DbContext
{
    protected override Task<PhaseOfEducation?> FindByCodeAsync(
        TContext context, string code, CancellationToken cancellationToken) =>
        context.Set<PhaseOfEducation>()
               .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);

    protected override PhaseOfEducation Create(string code, string name) =>
        new() { Code = code, Name = name };

    protected override void UpdateName(PhaseOfEducation entity, string name) =>
        entity.Name = name;
}