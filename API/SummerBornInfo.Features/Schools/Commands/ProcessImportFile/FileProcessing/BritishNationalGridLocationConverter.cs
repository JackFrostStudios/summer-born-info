namespace SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;

internal static class BritishNationalGridLocationConverter
{
    private const double Airy1830SemiMajorAxis = 6377563.396;
    private const double Airy1830SemiMinorAxis = 6356256.909;
    private const double NationalGridScaleFactor = 0.9996012717;
    private const double TrueOriginLatitudeRadians = 49d * Math.PI / 180d;
    private const double TrueOriginLongitudeRadians = -2d * Math.PI / 180d;
    private const double TrueOriginNorthing = -100000d;
    private const double TrueOriginEasting = 400000d;
    private const double LatitudeIterationTolerance = 0.00001d;
    private const double MaxEasting = 700000d;
    private const double MaxNorthing = 1300000d;

    public static NetTopologySuite.Geometries.Point? TryConvertToWgs84Point(string easting, string northing)
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

        var (osgbLatitude, osgbLongitude) = ConvertGridToOsgb36(parsedEasting, parsedNorthing);
        var (latitude, longitude) = ConvertOsgb36ToWgs84(osgbLatitude, osgbLongitude);

        return new NetTopologySuite.Geometries.Point(longitude, latitude) { SRID = 4326 };
    }

    private static (double Latitude, double Longitude) ConvertGridToOsgb36(double easting, double northing)
    {
        var eccentricitySquared = 1d - (Airy1830SemiMinorAxis * Airy1830SemiMinorAxis / (Airy1830SemiMajorAxis * Airy1830SemiMajorAxis));
        var n = (Airy1830SemiMajorAxis - Airy1830SemiMinorAxis) / (Airy1830SemiMajorAxis + Airy1830SemiMinorAxis);

        var latitude = TrueOriginLatitudeRadians;
        var meridionalArc = 0d;

        while (northing - TrueOriginNorthing - meridionalArc >= LatitudeIterationTolerance)
        {
            latitude += (northing - TrueOriginNorthing - meridionalArc) / (Airy1830SemiMajorAxis * NationalGridScaleFactor);
            meridionalArc = CalculateMeridionalArc(latitude, n);
        }

        var sinLatitude = Math.Sin(latitude);
        var cosLatitude = Math.Cos(latitude);
        var tangentLatitude = Math.Tan(latitude);
        var nu = Airy1830SemiMajorAxis * NationalGridScaleFactor / Math.Sqrt(1d - (eccentricitySquared * sinLatitude * sinLatitude));
        var rho = Airy1830SemiMajorAxis * NationalGridScaleFactor * (1d - eccentricitySquared)
            / Math.Pow(1d - (eccentricitySquared * sinLatitude * sinLatitude), 1.5d);
        var etaSquared = (nu / rho) - 1d;
        var deltaEasting = easting - TrueOriginEasting;

        var vii = tangentLatitude / (2d * rho * nu);
        var viii = tangentLatitude / (24d * rho * Math.Pow(nu, 3d))
            * (5d + (3d * tangentLatitude * tangentLatitude) + etaSquared - (9d * tangentLatitude * tangentLatitude * etaSquared));
        var ix = tangentLatitude / (720d * rho * Math.Pow(nu, 5d))
            * (61d + (90d * tangentLatitude * tangentLatitude) + (45d * Math.Pow(tangentLatitude, 4d)));
        var x = 1d / (cosLatitude * nu);
        var xi = 1d / (cosLatitude * 6d * Math.Pow(nu, 3d)) * ((nu / rho) + (2d * tangentLatitude * tangentLatitude));
        var xii = 1d / (cosLatitude * 120d * Math.Pow(nu, 5d))
            * (5d + (28d * tangentLatitude * tangentLatitude) + (24d * Math.Pow(tangentLatitude, 4d)));
        var xiia = 1d / (cosLatitude * 5040d * Math.Pow(nu, 7d))
            * (61d + (662d * tangentLatitude * tangentLatitude) + (1320d * Math.Pow(tangentLatitude, 4d)) + (720d * Math.Pow(tangentLatitude, 6d)));

        var latitudeRadians = latitude
            - (vii * Math.Pow(deltaEasting, 2d))
            + (viii * Math.Pow(deltaEasting, 4d))
            - (ix * Math.Pow(deltaEasting, 6d));

        var longitudeRadians = TrueOriginLongitudeRadians
            + (x * deltaEasting)
            - (xi * Math.Pow(deltaEasting, 3d))
            + (xii * Math.Pow(deltaEasting, 5d))
            - (xiia * Math.Pow(deltaEasting, 7d));

        return (latitudeRadians, longitudeRadians);
    }

    private static double CalculateMeridionalArc(double latitude, double n)
    {
        var ma = (1d + n + (1.25d * n * n) + (1.25d * n * n * n)) * (latitude - TrueOriginLatitudeRadians);
        var mb = ((3d * n) + (3d * n * n) + (2.625d * n * n * n))
            * Math.Sin(latitude - TrueOriginLatitudeRadians)
            * Math.Cos(latitude + TrueOriginLatitudeRadians);
        var mc = ((1.875d * n * n) + (1.875d * n * n * n))
            * Math.Sin(2d * (latitude - TrueOriginLatitudeRadians))
            * Math.Cos(2d * (latitude + TrueOriginLatitudeRadians));
        var md = 35d / 24d * n * n * n
            * Math.Sin(3d * (latitude - TrueOriginLatitudeRadians))
            * Math.Cos(3d * (latitude + TrueOriginLatitudeRadians));

        return Airy1830SemiMinorAxis * NationalGridScaleFactor * (ma - mb + mc - md);
    }

    private static (double Latitude, double Longitude) ConvertOsgb36ToWgs84(double latitudeRadians, double longitudeRadians)
    {
        const double airyEccentricitySquared = 1d - (Airy1830SemiMinorAxis * Airy1830SemiMinorAxis / (Airy1830SemiMajorAxis * Airy1830SemiMajorAxis));
        var airyNu = Airy1830SemiMajorAxis / Math.Sqrt(1d - (airyEccentricitySquared * Math.Pow(Math.Sin(latitudeRadians), 2d)));
        var x1 = airyNu * Math.Cos(latitudeRadians) * Math.Cos(longitudeRadians);
        var y1 = airyNu * Math.Cos(latitudeRadians) * Math.Sin(longitudeRadians);
        var z1 = airyNu * (1d - airyEccentricitySquared) * Math.Sin(latitudeRadians);

        const double translationX = 446.448d;
        const double translationY = -125.157d;
        const double translationZ = 542.060d;
        const double scaleFactor = -20.4894d * 0.000001d;
        const double rotationXRadians = 0.1502d / 3600d * Math.PI / 180d;
        const double rotationYRadians = 0.2470d / 3600d * Math.PI / 180d;
        const double rotationZRadians = 0.8421d / 3600d * Math.PI / 180d;

        var x2 = translationX + ((1d + scaleFactor) * x1) - (rotationZRadians * y1) + (rotationYRadians * z1);
        var y2 = translationY + (rotationZRadians * x1) + ((1d + scaleFactor) * y1) - (rotationXRadians * z1);
        var z2 = translationZ - (rotationYRadians * x1) + (rotationXRadians * y1) + ((1d + scaleFactor) * z1);

        const double wgs84SemiMajorAxis = 6378137d;
        const double wgs84SemiMinorAxis = 6356752.3141d;
        const double wgs84EccentricitySquared = 1d - (wgs84SemiMinorAxis * wgs84SemiMinorAxis / (wgs84SemiMajorAxis * wgs84SemiMajorAxis));

        var longitude = Math.Atan2(y2, x2);
        var horizontalDistance = Math.Sqrt((x2 * x2) + (y2 * y2));
        var latitude = Math.Atan2(z2, horizontalDistance * (1d - wgs84EccentricitySquared));
        double previousLatitude;

        do
        {
            previousLatitude = latitude;
            var nu = wgs84SemiMajorAxis / Math.Sqrt(1d - (wgs84EccentricitySquared * Math.Pow(Math.Sin(latitude), 2d)));
            latitude = Math.Atan2(z2 + (wgs84EccentricitySquared * nu * Math.Sin(latitude)), horizontalDistance);
        }
        while (Math.Abs(latitude - previousLatitude) > 0.000000000001d);

        return (latitude * 180d / Math.PI, longitude * 180d / Math.PI);
    }
}
