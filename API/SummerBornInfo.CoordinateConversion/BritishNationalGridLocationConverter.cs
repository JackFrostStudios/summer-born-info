namespace SummerBornInfo.CoordinateConversion;

public sealed class BritishNationalGridLocationConverter : IBritishNationalGridLocationConverter, IDisposable
{
    private const int BritishNationalGridEpsgCode = 27700;
    private const int Wgs84EpsgCode = 4326;
    private const double MaxEasting = 700000d;
    private const double MaxNorthing = 1300000d;
    private readonly object sync = new();
    private readonly TestHooks hooks;
    private SpatialReference? sourceSpatialReference;
    private SpatialReference? targetSpatialReference;
    private ThreadLocal<CoordinateTransformation>? coordinateTransformations;
    private int activeConversionCount;
    private bool disposeRequested;
    private bool disposed;

    public BritishNationalGridLocationConverter()
        : this(TestHooks.CreateDefault())
    {
    }

    internal BritishNationalGridLocationConverter(TestHooks testHooks)
    {
        hooks = testHooks ?? throw new ArgumentNullException(nameof(testHooks));
    }

    public Point? TryConvertToWgs84Point(string easting, string northing)
    {
        EnterActiveConversion();

        try
        {
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
            hooks.TransformPoint(coordinateTransformation, coordinates, parsedEasting, parsedNorthing, 0d);

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
        finally
        {
            ExitActiveConversion();
        }
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

            if (disposeRequested)
            {
                while (!disposed)
                {
                    _ = Monitor.Wait(sync);
                }

                return;
            }

            disposeRequested = true;

            while (activeConversionCount > 0)
            {
                hooks.OnDisposeBlockedByActiveConversion?.Invoke();
                _ = Monitor.Wait(sync);
            }

            threadLocalTransformations = coordinateTransformations;
            sourceReference = sourceSpatialReference;
            targetReference = targetSpatialReference;

            coordinateTransformations = null;
            sourceSpatialReference = null;
            targetSpatialReference = null;
            disposed = true;
            Monitor.PulseAll(sync);
        }

        if (threadLocalTransformations is not null)
        {
            foreach (var coordinateTransformation in threadLocalTransformations.Values)
            {
                hooks.DisposeCoordinateTransformation(coordinateTransformation);
            }

            threadLocalTransformations.Dispose();
        }

        if (targetReference is not null)
        {
            hooks.DisposeSpatialReference(targetReference);
        }

        if (sourceReference is not null)
        {
            hooks.DisposeSpatialReference(sourceReference);
        }
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
            if (coordinateTransformations is not null)
            {
                return;
            }

            var sourceReference = hooks.CreateSpatialReference(BritishNationalGridEpsgCode);

            try
            {
                var targetReference = hooks.CreateSpatialReference(Wgs84EpsgCode);

                try
                {
                    sourceSpatialReference = sourceReference;
                    targetSpatialReference = targetReference;
                    coordinateTransformations = new ThreadLocal<CoordinateTransformation>(
                        () => hooks.CreateCoordinateTransformation(sourceReference, targetReference),
                        trackAllValues: true);
                }
                catch
                {
                    hooks.DisposeSpatialReference(targetReference);
                    throw;
                }
            }
            catch
            {
                hooks.DisposeSpatialReference(sourceReference);
                throw;
            }
        }
    }

    private void EnterActiveConversion()
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposeRequested, this);
            activeConversionCount++;
        }
    }

    private void ExitActiveConversion()
    {
        lock (sync)
        {
            activeConversionCount--;

            if (disposeRequested && activeConversionCount == 0)
            {
                Monitor.PulseAll(sync);
            }
        }
    }

    internal sealed class TestHooks
    {
        internal Func<int, SpatialReference> CreateSpatialReference { get; init; } = CreateDefaultSpatialReference;
        internal Func<SpatialReference, SpatialReference, CoordinateTransformation> CreateCoordinateTransformation { get; init; } = CreateDefaultCoordinateTransformation;
        internal Action<CoordinateTransformation, double[], double, double, double> TransformPoint { get; init; } = static (coordinateTransformation, coordinates, easting, northing, height) =>
            coordinateTransformation.TransformPoint(coordinates, easting, northing, height);
        internal Action<SpatialReference> DisposeSpatialReference { get; init; } = static spatialReference => spatialReference.Dispose();
        internal Action<CoordinateTransformation> DisposeCoordinateTransformation { get; init; } = static coordinateTransformation => coordinateTransformation.Dispose();
        internal Action? OnDisposeBlockedByActiveConversion { get; init; }

        internal static TestHooks CreateDefault()
        {
            return new TestHooks();
        }
    }

    private static SpatialReference CreateDefaultSpatialReference(int epsgCode)
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

    private static CoordinateTransformation CreateDefaultCoordinateTransformation(
        SpatialReference sourceSpatialReference,
        SpatialReference targetSpatialReference)
    {
        return new CoordinateTransformation(sourceSpatialReference, targetSpatialReference);
    }
}
