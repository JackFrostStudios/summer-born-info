namespace SummerBornInfo.Features.Schools.Queries.GetSchoolByUrn;

public static class GetSchoolByUrnQueryValidator
{
    public static bool TryValidate(string? urn, out GetSchoolByUrnQuery query)
    {
        query = default!;

        if (!int.TryParse(urn, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedUrn) || parsedUrn <= 0)
        {
            return false;
        }

        query = new GetSchoolByUrnQuery(parsedUrn);
        return true;
    }
}
