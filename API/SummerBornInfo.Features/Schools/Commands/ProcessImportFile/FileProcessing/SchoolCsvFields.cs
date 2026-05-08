using nietras.SeparatedValues;

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

    public static SchoolCsvFields FromRow(SepReader.Row row)
    {
        return new()
        {
            URN = int.Parse(row["\"URN\""].ToString().Replace("\"", "")),
            EstablishmentNumber = int.Parse(row["\"EstablishmentNumber\""].ToString().Replace("\"", "")),
            EstablishmentName = row["\"EstablishmentName\""].ToString().Replace("\"", ""),
            LACode = row["\"LA (code)\""].ToString().Replace("\"", ""),
            LAName = row["\"LA (name)\""].ToString().Replace("\"", ""),
            EstablishmentTypeCode = row["\"TypeOfEstablishment (code)\""].ToString().Replace("\"", ""),
            EstablishmentTypeName = row["\"TypeOfEstablishment (name)\""].ToString().Replace("\"", ""),
            EstablishmentGroupCode = row["\"EstablishmentTypeGroup (code)\""].ToString().Replace("\"", ""),
            EstablishmentGroupName = row["\"EstablishmentTypeGroup (name)\""].ToString().Replace("\"", ""),
            EstablishmentStatusCode = row["\"EstablishmentStatus (code)\""].ToString().Replace("\"", ""),
            EstablishmentStatusName = row["\"EstablishmentStatus (name)\""].ToString().Replace("\"", ""),
            PhaseOfEducationCode = row["\"PhaseOfEducation (code)\""].ToString().Replace("\"", ""),
            PhaseOfEducationName = row["\"PhaseOfEducation (name)\""].ToString().Replace("\"", ""),
            OpenDate = row["\"OpenDate\""].ToString().Replace("\"", ""),
            CloseDate = row["\"CloseDate\""].ToString().Replace("\"", ""),
            UKPRN = row["\"UKPRN\""].ToString().Replace("\"", ""),
            Street = row["\"Street\""].ToString().Replace("\"", ""),
            Locality = row["\"Locality\""].ToString().Replace("\"", ""),
            AddressThree = row["\"Address3\""].ToString().Replace("\"", ""),
            Town = row["\"Town\""].ToString().Replace("\"", ""),
            County = row["\"County (name)\""].ToString().Replace("\"", ""),
            Postcode = row["\"Postcode\""].ToString().Replace("\"", ""),
        };
    }
}