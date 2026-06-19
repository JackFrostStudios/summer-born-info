namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

internal sealed record SchoolCsvFields
{
    public required int URN { get; init; }
    public required int EstablishmentNumber { get; init; }
    public required string EstablishmentName { get; init; }
    public required string LACode { get; init; }
    public required string LAName { get; init; }
    public required string EstablishmentTypeCode { get; init; }
    public required string EstablishmentTypeName { get; init; }
    public required string EstablishmentGroupCode { get; init; }
    public required string EstablishmentGroupName { get; init; }
    public required string EstablishmentStatusCode { get; init; }
    public required string EstablishmentStatusName { get; init; }
    public required string PhaseOfEducationCode { get; init; }
    public required string PhaseOfEducationName { get; init; }
    public required string OpenDate { get; init; }
    public required string CloseDate { get; init; }
    public required string UKPRN { get; init; }
    public required string Street { get; init; }
    public required string Locality { get; init; }
    public required string AddressThree { get; init; }
    public required string Town { get; init; }
    public required string County { get; init; }
    public required string Postcode { get; init; }
    public required string Easting { get; init; }
    public required string Northing { get; init; }

    public static SchoolCsvFields FromRow(SepReader.Row row)
    {
        return new()
        {
            URN = ParseInt(row, "URN"),
            EstablishmentNumber = ParseInt(row, "EstablishmentNumber"),
            EstablishmentName = ParseString(row, "EstablishmentName"),
            LACode = ParseString(row, "LA (code)"),
            LAName = ParseString(row, "LA (name)"),
            EstablishmentTypeCode = ParseString(row, "TypeOfEstablishment (code)"),
            EstablishmentTypeName = ParseString(row, "TypeOfEstablishment (name)"),
            EstablishmentGroupCode = ParseString(row, "EstablishmentTypeGroup (code)"),
            EstablishmentGroupName = ParseString(row, "EstablishmentTypeGroup (name)"),
            EstablishmentStatusCode = ParseString(row, "EstablishmentStatus (code)"),
            EstablishmentStatusName = ParseString(row, "EstablishmentStatus (name)"),
            PhaseOfEducationCode = ParseString(row, "PhaseOfEducation (code)"),
            PhaseOfEducationName = ParseString(row, "PhaseOfEducation (name)"),
            OpenDate = ParseString(row, "OpenDate"),
            CloseDate = ParseString(row, "CloseDate"),
            UKPRN = ParseString(row, "UKPRN"),
            Street = ParseString(row, "Street"),
            Locality = ParseString(row, "Locality"),
            AddressThree = ParseString(row, "Address3"),
            Town = ParseString(row, "Town"),
            County = ParseString(row, "County (name)"),
            Postcode = ParseString(row, "Postcode"),
            Easting = ParseString(row, "Easting"),
            Northing = ParseString(row, "Northing"),
        };
    }

    private static int ParseInt(SepReader.Row row, string columnName)
    {
        return int.Parse(ParseString(row, columnName), CultureInfo.InvariantCulture);
    }

    private static string ParseString(SepReader.Row row, string columnName)
    {
        return row[$"\"{columnName}\""].ToString().Replace("\"", string.Empty, StringComparison.Ordinal);
    }
}
