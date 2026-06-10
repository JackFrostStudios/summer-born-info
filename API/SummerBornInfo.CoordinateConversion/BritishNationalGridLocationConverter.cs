namespace SummerBornInfo.CoordinateConversion;

public static class BritishNationalGridLocationConverter
{
    private const int BritishNationalGridEpsgCode = 27700;
    private const int Wgs84EpsgCode = 4326;
    private const double MaxEasting = 700000d;
    private const double MaxNorthing = 1300000d;

    public static Point? TryConvertToWgs84Point(string easting, string northing)
    {
        if (!double.TryParse(easting, CultureInfo.InvariantCulture, out var parsedEasting)
            || !double.TryParse(northing, CultureInfo.InvariantCulture, out var parsedNorthing)
            || !double.IsFinite(parsedEasting)
            || !double.IsFinite(parsedNorthing)
            || parsedEasting is < 0 or > MaxEasting
            || parsedNorthing is < 0 or > MaxNorthing)
        {
            return null;
        }

        using var sourceSpatialReference = CreateSpatialReference(BritishNationalGridEpsgCode);
        using var targetSpatialReference = CreateSpatialReference(Wgs84EpsgCode);
        using var coordinateTransformation = new CoordinateTransformation(sourceSpatialReference, targetSpatialReference);
        var coordinates = new double[3];
        coordinateTransformation.TransformPoint(coordinates, parsedEasting, parsedNorthing, 0d);

        var longitude = coordinates[0];
        var latitude = coordinates[1];

        if (!double.IsFinite(longitude)
            || !double.IsFinite(latitude)
            || longitude is < -180d or > 180d
            || latitude is < -90d or > 90d)
        {
            return null;
        }

        return new Point(longitude, latitude) { SRID = 4326 };
    }

    private static SpatialReference CreateSpatialReference(int epsgCode)
    {
        var spatialReference = new SpatialReference(string.Empty);
        var importResult = spatialReference.ImportFromEPSG(epsgCode);

        if (importResult != 0)
        {
            spatialReference.Dispose();
            throw new InvalidOperationException($"Unable to load EPSG:{epsgCode.ToString(CultureInfo.InvariantCulture)} for school coordinate conversion.");
        }

        spatialReference.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return spatialReference;
    }
}
