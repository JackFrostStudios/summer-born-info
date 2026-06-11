namespace SummerBornInfo.CoordinateConversion;

public interface IBritishNationalGridLocationConverter
{
    Point? TryConvertToWgs84Point(string easting, string northing);
}
