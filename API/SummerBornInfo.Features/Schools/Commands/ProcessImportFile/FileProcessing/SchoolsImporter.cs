using nietras.SeparatedValues;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing.LookupImporter;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class SchoolsImporter<TContext>(TContext context) where TContext : DbContext
{
    private readonly TContext _context = context;
    private readonly EstablishmentGroupImporter<TContext> _groupImporter = new(context);
    private readonly EstablishmentStatusImporter<TContext> _statusImporter = new(context);
    private readonly EstablishmentTypeImporter<TContext> _typeImporter = new(context);
    private readonly LocalAuthorityImporter<TContext> _laImporter = new(context);
    private readonly PhaseOfEducationImporter<TContext> _phaseImporter = new(context);

    public async IAsyncEnumerable<SchoolImportResult> ImportAsync(
        Stream csvStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var streamReader = new StreamReader(csvStream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        using var reader = Sep.New(',').Reader(o => o with { Trim = SepTrim.All }).From(streamReader);

        int lineNumber = 1;

        foreach (var row in reader)
        {
            lineNumber++;
            SchoolImportResult result;

            try
            {
                await ProcessRowAsync(SchoolCsvFields.FromRow(row), cancellationToken);
                result = new SchoolImportResult
                {
                    LineNumber = lineNumber,
                    Succeeded = true,
                };
            }
            catch (Exception ex)
            {
                result = new SchoolImportResult
                {
                    LineNumber = lineNumber,
                    Succeeded = false,
                    ErrorMessage = ex.Message,
                };
            }

            yield return result;
        }
    }

    private async Task ProcessRowAsync(SchoolCsvFields row, CancellationToken cancellationToken)
    {
        var localAuthority = await _laImporter.UpsertAsync(row.LACode, row.LAName, cancellationToken);
        var establishmentType = await _typeImporter.UpsertAsync(row.EstablishmentTypeCode, row.EstablishmentTypeName, cancellationToken);
        var establishmentGroup = await _groupImporter.UpsertAsync(row.EstablishmentGroupCode, row.EstablishmentGroupName, cancellationToken);
        var establishmentStatus = await _statusImporter.UpsertAsync(row.EstablishmentStatusCode, row.EstablishmentStatusName, cancellationToken);
        var phaseOfEducation = await _phaseImporter.UpsertAsync(row.PhaseOfEducationCode, row.PhaseOfEducationName, cancellationToken);

        var school = await _context.Set<School>()
            .FirstOrDefaultAsync(s => s.URN == row.URN, cancellationToken);

        if (school is null)
        {
            school = new School
            {
                URN = row.URN,
                EstablishmentNumber = row.EstablishmentNumber,
                Name = row.EstablishmentName,
                Address = BuildAddress(row),
                OpenDate = ParseDate(row.OpenDate),
                CloseDate = ParseDate(row.CloseDate),
                UKPRN = ParseNullableInt(row.UKPRN),
                LocalAuthority = localAuthority,
                EstablishmentType = establishmentType,
                EstablishmentGroup = establishmentGroup,
                EstablishmentStatus = establishmentStatus,
                PhaseOfEducation = phaseOfEducation,
            };

            await _context.Set<School>().AddAsync(school, cancellationToken);
        }
        else
        {
            school.EstablishmentNumber = row.EstablishmentNumber;
            school.Name = row.EstablishmentName;
            school.OpenDate = ParseDate(row.OpenDate);
            school.CloseDate = ParseDate(row.CloseDate);
            school.UKPRN = ParseNullableInt(row.UKPRN);

            school.Address.Street = NullIfEmpty(row.Street);
            school.Address.Locality = NullIfEmpty(row.Locality);
            school.Address.AddressThree = NullIfEmpty(row.AddressThree);
            school.Address.Town = row.Town;
            school.Address.County = NullIfEmpty(row.County);
            school.Address.PostCode = row.Postcode;

            school.LocalAuthority = localAuthority;
            school.EstablishmentType = establishmentType;
            school.EstablishmentGroup = establishmentGroup;
            school.EstablishmentStatus = establishmentStatus;
            school.PhaseOfEducation = phaseOfEducation;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _context.Entry(school.Address).State = EntityState.Detached;
        _context.Entry(school).State = EntityState.Detached;
    }

    private static SchoolAddress BuildAddress(SchoolCsvFields row) => new()
    {
        Street = NullIfEmpty(row.Street),
        Locality = NullIfEmpty(row.Locality),
        AddressThree = NullIfEmpty(row.AddressThree),
        Town = row.Town,
        County = NullIfEmpty(row.County),
        PostCode = row.Postcode,
    };

    private static DateOnly? ParseDate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return DateOnly.TryParseExact(raw, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static int? ParseNullableInt(string raw) =>
        int.TryParse(raw, out var value) ? value : null;

    private static string? NullIfEmpty(string raw) =>
        string.IsNullOrWhiteSpace(raw) ? null : raw;
}
