namespace SummerBornInfo.CoordinateConversion;

public sealed class BritishNationalGridLocationConverter : IBritishNationalGridLocationConverter, IDisposable
{
    private const int BritishNationalGridEpsgCode = 27700;
    private const int Wgs84EpsgCode = 4326;
    private const double MaxEasting = 700000d;
    private const double MaxNorthing = 1300000d;
    private readonly Lock sync = new();
    private SpatialReference? sourceSpatialReference;
    private SpatialReference? targetSpatialReference;
    private ThreadLocal<CoordinateTransformation>? coordinateTransformations;
    private bool disposed;

    public Point? TryConvertToWgs84Point(string easting, string northing)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        GdalRuntimeConfiguration.Configure();

        if (!double.TryParse(easting, CultureInfo.InvariantCulture, out var parsedEasting)
            || !double.TryParse(northing, CultureInfo.InvariantCulture, out var parsedNorthing)
            || !double.IsFinite(parsedEasting)
            || !double.IsFinite(parsedNorthing)
            || parsedEasting is < 0 or > MaxEasting
            || parsedNorthing is < 0 or > MaxNorthing)
        {
            return null;
        }

        var coordinateTransformation = GetCoordinateTransformation();
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

    public void Dispose()
    {
        ThreadLocal<CoordinateTransformation>? threadLocalTransformations;
        SpatialReference? sourceReference;
        SpatialReference? targetReference;

        lock (sync)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            threadLocalTransformations = coordinateTransformations;
            sourceReference = sourceSpatialReference;
            targetReference = targetSpatialReference;

            coordinateTransformations = null;
            sourceSpatialReference = null;
            targetSpatialReference = null;
        }

        if (threadLocalTransformations is not null)
        {
            foreach (var coordinateTransformation in threadLocalTransformations.Values)
            {
                coordinateTransformation.Dispose();
            }

            threadLocalTransformations.Dispose();
        }

        targetReference?.Dispose();
        sourceReference?.Dispose();
    }

    private CoordinateTransformation GetCoordinateTransformation()
    {
        if (coordinateTransformations is null)
        {
            EnsureCoordinateTransformationCache();
        }

        return coordinateTransformations!.Value
            ?? throw new InvalidOperationException("Unable to create a coordinate transformation for school coordinate conversion.");
    }

    private void EnsureCoordinateTransformationCache()
    {
        if (coordinateTransformations is not null)
        {
            return;
        }

        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (coordinateTransformations is not null)
            {
                return;
            }

            var sourceReference = CreateSpatialReference(BritishNationalGridEpsgCode);

            try
            {
                var targetReference = CreateSpatialReference(Wgs84EpsgCode);

                try
                {
                    sourceSpatialReference = sourceReference;
                    targetSpatialReference = targetReference;
                    coordinateTransformations = new ThreadLocal<CoordinateTransformation>(
                        () => CreateCoordinateTransformation(sourceReference, targetReference),
                        trackAllValues: true);
                }
                catch
                {
                    targetReference.Dispose();
                    throw;
                }
            }
            catch
            {
                sourceReference.Dispose();
                throw;
            }
        }
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

    private static CoordinateTransformation CreateCoordinateTransformation(
        SpatialReference sourceSpatialReference,
        SpatialReference targetSpatialReference)
    {
        return new CoordinateTransformation(sourceSpatialReference, targetSpatialReference);
    }
}
