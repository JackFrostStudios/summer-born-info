namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed partial class SchoolsImporter<TContext>(TContext context, ILogger<SchoolsImporter<TContext>> logger) : ISchoolsImporter where TContext : DbContext
{
    private readonly TContext _context = context;
    private readonly EstablishmentGroupImporter<TContext> _groupImporter = new(context);
    private readonly EstablishmentStatusImporter<TContext> _statusImporter = new(context);
    private readonly EstablishmentTypeImporter<TContext> _typeImporter = new(context);
    private readonly LocalAuthorityImporter<TContext> _laImporter = new(context);
    private readonly PhaseOfEducationImporter<TContext> _phaseImporter = new(context);

    public IAsyncEnumerable<SchoolImportResult> ImportAsync(
        Guid schoolBulkImportRequestId,
        Stream csvStream,
        CancellationToken cancellationToken)
    {
        return ImportAsync(schoolBulkImportRequestId, csvStream, processedRowsToSkip: 0, cancellationToken);
    }

    public async IAsyncEnumerable<SchoolImportResult> ImportAsync(
        Guid schoolBulkImportRequestId,
        Stream csvStream,
        int processedRowsToSkip = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using StreamReader streamReader = new(csvStream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        using var reader = await Sep.New(',').Reader(o => o with { Trim = SepTrim.All }).FromAsync(streamReader, cancellationToken);

        var lineNumber = 1;

        foreach (var row in reader)
        {
            lineNumber++;

            if (processedRowsToSkip > 0)
            {
                processedRowsToSkip--;
                continue;
            }

            SchoolImportResult result;
            using var activity = SchoolBulkImportTelemetry.ActivitySource.StartActivity(SchoolBulkImportTelemetry.ActivityName);
            _ = (activity?.SetTag("schoolBulkImport.request_id", schoolBulkImportRequestId));
            _ = (activity?.SetTag("schoolBulkImport.row_number", lineNumber));

            try
            {
                var schoolCsvFields = ParseRow(row);
                result = await ImportParsedRowAsync(schoolBulkImportRequestId, schoolCsvFields, lineNumber, activity, cancellationToken);
            }
            catch (SchoolBulkImportRowParseException ex)
            {
                MarkRowFailure(activity, ex.Message);
                result = new SchoolImportResult { LineNumber = lineNumber, Succeeded = false, ErrorMessage = ex.Message };
            }

            yield return result;
        }
    }

    private async Task<SchoolImportResult> ImportParsedRowAsync(
        Guid schoolBulkImportRequestId,
        SchoolCsvFields schoolCsvFields,
        int lineNumber,
        Activity? activity,
        CancellationToken cancellationToken)
    {
        try
        {
            await ProcessRowAsync(schoolCsvFields, cancellationToken);
            MarkRowProcessed(activity);
            return new SchoolImportResult { LineNumber = lineNumber, Succeeded = true };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SchoolBulkImportProcessingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogUnexpectedProcessingError(logger, lineNumber, schoolBulkImportRequestId, ex.Message, ex);
            MarkRowAborted(activity);
            throw new SchoolBulkImportProcessingException(ex);
        }
    }

    private static SchoolCsvFields ParseRow(SepReader.Row row)
    {
        try
        {
            return SchoolCsvFields.FromRow(row);
        }
        catch (Exception ex)
        {
            throw new SchoolBulkImportRowParseException($"Unable to parse CSV row. {ex.Message}", ex);
        }
    }

    private static void MarkRowProcessed(Activity? activity)
    {
        _ = (activity?.SetTag("schoolBulkImport.outcome", "processed"));
        _ = (activity?.SetStatus(ActivityStatusCode.Ok));
    }

    private static void MarkRowFailure(Activity? activity, string errorMessage)
    {
        _ = (activity?.SetTag("schoolBulkImport.outcome", "failed"));
        _ = (activity?.SetStatus(ActivityStatusCode.Error, errorMessage));
        _ = (activity?.SetTag("schoolBulkImport.error", errorMessage));
    }

    private static void MarkRowAborted(Activity? activity)
    {
        _ = (activity?.SetTag("schoolBulkImport.outcome", "aborted"));
        _ = (activity?.SetStatus(ActivityStatusCode.Error, "The import file could not be processed. Please try again."));
    }

    private async Task ProcessRowAsync(SchoolCsvFields row, CancellationToken cancellationToken)
    {
        var localAuthority = await _laImporter.UpsertAsync(row.LACode, row.LAName, cancellationToken);
        var establishmentType = await _typeImporter.UpsertAsync(row.EstablishmentTypeCode, row.EstablishmentTypeName, cancellationToken);
        var establishmentGroup = await _groupImporter.UpsertAsync(row.EstablishmentGroupCode, row.EstablishmentGroupName, cancellationToken);
        var establishmentStatus = await _statusImporter.UpsertAsync(row.EstablishmentStatusCode, row.EstablishmentStatusName, cancellationToken);
        var phaseOfEducation = await _phaseImporter.UpsertAsync(row.PhaseOfEducationCode, row.PhaseOfEducationName, cancellationToken);
        var location = BritishNationalGridLocationConverter.TryConvertToWgs84Point(row.Easting, row.Northing);

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
                Location = location,
                OpenDate = ParseDate(row.OpenDate),
                CloseDate = ParseDate(row.CloseDate),
                UKPRN = ParseNullableInt(row.UKPRN),
                LocalAuthority = localAuthority,
                EstablishmentType = establishmentType,
                EstablishmentGroup = establishmentGroup,
                EstablishmentStatus = establishmentStatus,
                PhaseOfEducation = phaseOfEducation,
            };

            _ = await _context.Set<School>().AddAsync(school, cancellationToken);
        }
        else
        {
            school.EstablishmentNumber = row.EstablishmentNumber;
            school.Name = row.EstablishmentName;
            school.OpenDate = ParseDate(row.OpenDate);
            school.CloseDate = ParseDate(row.CloseDate);
            school.UKPRN = ParseNullableInt(row.UKPRN);
            school.Location = location;

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

        _ = await _context.SaveChangesAsync(cancellationToken);

        _context.Entry(school.Address).State = EntityState.Detached;
        _context.Entry(school).State = EntityState.Detached;
    }

    private static SchoolAddress BuildAddress(SchoolCsvFields row)
    {
        return new()
        {
            Street = NullIfEmpty(row.Street),
            Locality = NullIfEmpty(row.Locality),
            AddressThree = NullIfEmpty(row.AddressThree),
            Town = row.Town,
            County = NullIfEmpty(row.County),
            PostCode = row.Postcode,
        };
    }

    private static DateOnly? ParseDate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return DateOnly.TryParseExact(raw, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static int? ParseNullableInt(string raw)
    {
        return int.TryParse(raw, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static string? NullIfEmpty(string raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? null : raw;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unexpected error processing school bulk import row {LineNumber} for request {SchoolBulkImportRequestId}: {ErrorMessage}")]
    private static partial void LogUnexpectedProcessingError(
        ILogger logger,
        int lineNumber,
        Guid schoolBulkImportRequestId,
        string errorMessage,
        Exception exception);
}
