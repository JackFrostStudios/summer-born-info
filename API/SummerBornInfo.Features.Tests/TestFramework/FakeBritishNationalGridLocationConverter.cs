namespace SummerBornInfo.Features.Tests.TestFramework;

public sealed class FakeBritishNationalGridLocationConverter : IBritishNationalGridLocationConverter
{
    private readonly Dictionary<CoordinateKey, Func<Point?>> _configuredResults = [];
    private Func<string, string, Point?> _defaultResultFactory = static (_, _) => null;

    public Point? TryConvertToWgs84Point(string easting, string northing)
    {
        return _configuredResults.TryGetValue(new CoordinateKey(easting, northing), out var configuredResultFactory)
            ? configuredResultFactory()
            : _defaultResultFactory(easting, northing);
    }

    public FakeBritishNationalGridLocationConverter Returns(string easting, string northing, Point? point)
    {
        _configuredResults[new CoordinateKey(easting, northing)] = CreatePointFactory(point);
        return this;
    }

    public FakeBritishNationalGridLocationConverter Throws(string easting, string northing, Exception exception)
    {
        _configuredResults[new CoordinateKey(easting, northing)] = () => throw exception;
        return this;
    }

    public FakeBritishNationalGridLocationConverter ReturnsByDefault(Point? point)
    {
        var pointFactory = CreatePointFactory(point);
        _defaultResultFactory = (_, _) => pointFactory();
        return this;
    }

    public FakeBritishNationalGridLocationConverter ThrowsByDefault(Exception exception)
    {
        _defaultResultFactory = (_, _) => throw exception;
        return this;
    }

    public static FakeBritishNationalGridLocationConverter ForExampleImportFile()
    {
        return new FakeBritishNationalGridLocationConverter()
            .Returns("533498", "181201", CreateExampleAldgatePoint())
            .Returns("533523", "181201", CreateExampleAldgatePoint())
            .Returns("528515", "184869", CreateExampleSherbornePoint());
    }

    public static Point CreateExampleAldgatePoint()
    {
        return CreatePoint(-0.0765d, 51.5154d);
    }

    public static Point CreateExampleSherbornePoint()
    {
        return CreatePoint(-0.1512d, 51.5456d);
    }

    public static Point CreatePoint(double longitude, double latitude)
    {
        return new Point(longitude, latitude) { SRID = 4326 };
    }

    private static Func<Point?> CreatePointFactory(Point? point)
    {
        if (point is null)
        {
            return static () => null;
        }

        return () => CreatePoint(point.X, point.Y);
    }

    private sealed record CoordinateKey(string Easting, string Northing);
}
