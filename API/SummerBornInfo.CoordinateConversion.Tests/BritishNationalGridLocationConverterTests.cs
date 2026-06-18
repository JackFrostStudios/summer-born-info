namespace SummerBornInfo.CoordinateConversion.Tests;

[TestCaseOrderer(typeof(BritishNationalGridLocationConverterTestCaseOrderer))]
public sealed class BritishNationalGridLocationConverterTests : IDisposable
{
    private const string ProjDatabaseFileName = "proj.db";
    private const string Ostn15GridFileName = "uk_os_OSTN15_NTv2_OSGBtoETRS.tif";
    private static readonly string AppBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);
    private readonly BritishNationalGridLocationConverter converter = new();

    [Fact]
    public void GivenConverter_WhenConversionIsAttemptedWithoutManualRuntimeBootstrap_ThenBundledOfflineRuntimeDataAndGridShiftsAreConfigured()
    {
        var point = converter.TryConvertToWgs84Point("533523", "181201");
        var gridShiftSearchPath = Path.Combine(AppBaseDirectory, "GridShifts");
        var configuredSearchPaths = GetConfiguredProjSearchPaths();

        Assert.NotNull(point);
        Assert.False(Osr.GetPROJEnableNetwork(), "Expected PROJ network access to remain disabled.");
        Assert.True(File.Exists(Path.Combine(gridShiftSearchPath, Ostn15GridFileName)));
        Assert.True(
            ContainsLocalFile(configuredSearchPaths, ProjDatabaseFileName),
            $"Expected configured PROJ search paths to expose a local '{ProjDatabaseFileName}'.");
        Assert.True(
            configuredSearchPaths.Contains(gridShiftSearchPath, StringComparer.OrdinalIgnoreCase),
            $"Expected configured PROJ search paths to include '{gridShiftSearchPath}'.");
    }

    [Fact]
    public void GivenValidBritishNationalGridCoordinates_WhenTryConvertToWgs84Point_ThenWgs84PointIsReturned()
    {
        // Act
        var point = converter.TryConvertToWgs84Point("533523", "181201");

        // Assert
        Assert.NotNull(point);
        Assert.Equal(4326, point.SRID);
        Assert.InRange(point.X, -0.09d, -0.06d);
        Assert.InRange(point.Y, 51.50d, 51.53d);
    }

    [Fact]
    public void GivenValidBritishNationalGridCoordinatesWhereGridShiftMatters_WhenTryConvertToWgs84Point_ThenWgs84PointIsReturned()
    {
        // Act
        var point = converter.TryConvertToWgs84Point("433962", "300363");

        // Assert
        Assert.NotNull(point);
        Assert.Equal(4326, point.SRID);
        Assert.InRange(point.X, -1.501d, -1.499d);
        Assert.InRange(point.Y, 52.599d, 52.601d);
    }

    [Theory]
    [InlineData("invalid", "181201")]
    [InlineData("533523", "invalid")]
    [InlineData("", "181201")]
    [InlineData("533523", "")]
    [InlineData("Infinity", "181201")]
    public void GivenInvalidCoordinateValues_WhenTryConvertToWgs84Point_ThenNullIsReturned(string easting, string northing)
    {
        // Act
        var point = converter.TryConvertToWgs84Point(easting, northing);

        // Assert
        Assert.Null(point);
    }

    [Theory]
    [InlineData("-1", "181201")]
    [InlineData("700001", "181201")]
    [InlineData("533523", "-1")]
    [InlineData("533523", "1300001")]
    public void GivenOutOfRangeBritishNationalGridCoordinates_WhenTryConvertToWgs84Point_ThenNullIsReturned(string easting, string northing)
    {
        // Act
        var point = converter.TryConvertToWgs84Point(easting, northing);

        // Assert
        Assert.Null(point);
    }

    [Fact]
    public void GivenLifecycleTrackingHooks_WhenConverterIsUsedAndDisposed_ThenCreationAndDisposalCountsAreObservable()
    {
        var createdSpatialReferenceCount = 0;
        var createdTransformationCount = 0;
        var disposedSpatialReferenceCount = 0;
        var disposedTransformationCount = 0;

        using var instrumentedConverter = new BritishNationalGridLocationConverter(new BritishNationalGridLocationConverter.TestHooks
        {
            CreateSpatialReference = epsgCode =>
            {
                createdSpatialReferenceCount++;
                return CreateSpatialReference(epsgCode);
            },
            CreateCoordinateTransformation = (sourceSpatialReference, targetSpatialReference) =>
            {
                createdTransformationCount++;
                return new CoordinateTransformation(sourceSpatialReference, targetSpatialReference);
            },
            DisposeSpatialReference = spatialReference =>
            {
                disposedSpatialReferenceCount++;
                spatialReference.Dispose();
            },
            DisposeCoordinateTransformation = coordinateTransformation =>
            {
                disposedTransformationCount++;
                coordinateTransformation.Dispose();
            },
        });

        var point = instrumentedConverter.TryConvertToWgs84Point("533523", "181201");

        Assert.NotNull(point);
        Assert.Equal(2, createdSpatialReferenceCount);
        Assert.Equal(1, createdTransformationCount);
        Assert.Equal(0, disposedSpatialReferenceCount);
        Assert.Equal(0, disposedTransformationCount);

        instrumentedConverter.Dispose();

        Assert.Equal(2, disposedSpatialReferenceCount);
        Assert.Equal(1, disposedTransformationCount);
    }

    [Fact]
    public void GivenLifecycleTrackingHooks_WhenValidCoordinatesAreConvertedRepeatedlyOnTheSameThread_ThenTheTransformationIsReused()
    {
        var createdSpatialReferenceCount = 0;
        var createdTransformationCount = 0;

        using var instrumentedConverter = new BritishNationalGridLocationConverter(new BritishNationalGridLocationConverter.TestHooks
        {
            CreateSpatialReference = epsgCode =>
            {
                createdSpatialReferenceCount++;
                return CreateSpatialReference(epsgCode);
            },
            CreateCoordinateTransformation = (sourceSpatialReference, targetSpatialReference) =>
            {
                createdTransformationCount++;
                return new CoordinateTransformation(sourceSpatialReference, targetSpatialReference);
            },
        });

        var firstPoint = instrumentedConverter.TryConvertToWgs84Point("533523", "181201");
        var secondPoint = instrumentedConverter.TryConvertToWgs84Point("433962", "300363");

        Assert.NotNull(firstPoint);
        Assert.NotNull(secondPoint);
        Assert.Equal(2, createdSpatialReferenceCount);
        Assert.Equal(1, createdTransformationCount);
        Assert.InRange(firstPoint.X, -0.09d, -0.06d);
        Assert.InRange(firstPoint.Y, 51.50d, 51.53d);
        Assert.InRange(secondPoint.X, -1.501d, -1.499d);
        Assert.InRange(secondPoint.Y, 52.599d, 52.601d);
    }

    [Fact]
    public void GivenLifecycleTrackingHooks_WhenValidCoordinatesAreConvertedOnSeparateThreads_ThenEachThreadCreatesItsOwnTransformation()
    {
        var createdSpatialReferenceCount = 0;
        var createdTransformationCount = 0;
        var transformationCreationThreadIds = new ConcurrentBag<int>();
        var conversionThreadIds = new ConcurrentBag<int>();
        var points = new ConcurrentBag<Point>();
        var failures = new ConcurrentQueue<Exception>();
        using var startGate = new ManualResetEventSlim(initialState: false);

        using var instrumentedConverter = new BritishNationalGridLocationConverter(new BritishNationalGridLocationConverter.TestHooks
        {
            CreateSpatialReference = epsgCode =>
            {
                _ = Interlocked.Increment(ref createdSpatialReferenceCount);
                return CreateSpatialReference(epsgCode);
            },
            CreateCoordinateTransformation = (sourceSpatialReference, targetSpatialReference) =>
            {
                transformationCreationThreadIds.Add(Environment.CurrentManagedThreadId);
                _ = Interlocked.Increment(ref createdTransformationCount);
                return new CoordinateTransformation(sourceSpatialReference, targetSpatialReference);
            },
        });

        var firstThread = CreateConversionThread(
            instrumentedConverter,
            startGate,
            "533523",
            "181201",
            conversionThreadIds,
            points,
            failures);
        var secondThread = CreateConversionThread(
            instrumentedConverter,
            startGate,
            "433962",
            "300363",
            conversionThreadIds,
            points,
            failures);

        firstThread.Start();
        secondThread.Start();
        startGate.Set();
        firstThread.Join();
        secondThread.Join();

        if (failures.TryDequeue(out var failure))
        {
            throw new AggregateException(failures.Prepend(failure));
        }

        Assert.Equal(2, points.Count);
        Assert.Equal(2, createdSpatialReferenceCount);
        Assert.Equal(2, createdTransformationCount);
        Assert.Equal(2, conversionThreadIds.Distinct().Count());
        Assert.Equal(2, transformationCreationThreadIds.Distinct().Count());
    }

    [Fact]
    public void GivenActiveConversion_WhenDisposeRunsConcurrently_ThenItWaitsBeforeReleasingNativeResources()
    {
        var disposedSpatialReferenceCount = new Counter();
        var disposedTransformationCount = new Counter();
        var disposeWaitSignalCount = new Counter();
        Point? convertedPoint = null;
        var failures = new ConcurrentQueue<Exception>();
        using var transformStarted = new ManualResetEventSlim(initialState: false);
        using var allowTransformToComplete = new ManualResetEventSlim(initialState: false);
        using var disposeWaiting = new ManualResetEventSlim(initialState: false);
        using var disposeCompleted = new ManualResetEventSlim(initialState: false);
        using var instrumentedConverter = CreateDisposeRaceInstrumentedConverter(
            transformStarted,
            allowTransformToComplete,
            disposeWaiting,
            disposedSpatialReferenceCount,
            disposedTransformationCount,
            disposeWaitSignalCount);

        var conversionThread = CreateFailureCapturingThread(
            () => convertedPoint = instrumentedConverter.TryConvertToWgs84Point("533523", "181201"),
            failures);
        var disposeThread = CreateFailureCapturingThread(
            () =>
            {
                instrumentedConverter.Dispose();
                disposeCompleted.Set();
            },
            failures);

        conversionThread.Start();
        Assert.True(transformStarted.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        disposeThread.Start();
        Assert.True(disposeWaiting.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.False(disposeCompleted.IsSet);
        Assert.Equal(0, disposedTransformationCount.Value);
        Assert.Equal(0, disposedSpatialReferenceCount.Value);
        Assert.True(Volatile.Read(ref disposeWaitSignalCount.Value) > 0);

        allowTransformToComplete.Set();
        Assert.True(conversionThread.Join(TimeSpan.FromSeconds(5)));
        Assert.True(disposeThread.Join(TimeSpan.FromSeconds(5)));
        ThrowIfAnyFailures(failures);

        Assert.NotNull(convertedPoint);
        Assert.True(disposeCompleted.IsSet);
        Assert.Equal(1, disposedTransformationCount.Value);
        Assert.Equal(2, disposedSpatialReferenceCount.Value);
    }

    private static Thread CreateConversionThread(
        BritishNationalGridLocationConverter converter,
        ManualResetEventSlim startGate,
        string easting,
        string northing,
        ConcurrentBag<int> conversionThreadIds,
        ConcurrentBag<Point> points,
        ConcurrentQueue<Exception> failures)
    {
        return new Thread(() =>
        {
            try
            {
                startGate.Wait(TestContext.Current.CancellationToken);
                conversionThreadIds.Add(Environment.CurrentManagedThreadId);
                var point = converter.TryConvertToWgs84Point(easting, northing);
                Assert.NotNull(point);
                points.Add(point);
            }
            catch (Exception exception)
            {
                failures.Enqueue(exception);
            }
        });
    }

    private static Thread CreateFailureCapturingThread(Action action, ConcurrentQueue<Exception> failures)
    {
        return new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failures.Enqueue(exception);
            }
        });
    }

    private static void ThrowIfAnyFailures(ConcurrentQueue<Exception> failures)
    {
        if (failures.TryDequeue(out var failure))
        {
            throw new AggregateException(failures.Prepend(failure));
        }
    }

    private static BritishNationalGridLocationConverter CreateDisposeRaceInstrumentedConverter(
        ManualResetEventSlim transformStarted,
        ManualResetEventSlim allowTransformToComplete,
        ManualResetEventSlim disposeWaiting,
        Counter disposedSpatialReferenceCount,
        Counter disposedTransformationCount,
        Counter disposeWaitSignalCount)
    {
        return new BritishNationalGridLocationConverter(new BritishNationalGridLocationConverter.TestHooks
        {
            TransformPoint = (coordinateTransformation, coordinates, easting, northing, height) =>
            {
                transformStarted.Set();
                allowTransformToComplete.Wait(TestContext.Current.CancellationToken);
                coordinateTransformation.TransformPoint(coordinates, easting, northing, height);
            },
            DisposeSpatialReference = spatialReference =>
            {
                _ = Interlocked.Increment(ref disposedSpatialReferenceCount.Value);
                spatialReference.Dispose();
            },
            DisposeCoordinateTransformation = coordinateTransformation =>
            {
                _ = Interlocked.Increment(ref disposedTransformationCount.Value);
                coordinateTransformation.Dispose();
            },
            OnDisposeBlockedByActiveConversion = () =>
            {
                _ = Interlocked.Increment(ref disposeWaitSignalCount.Value);
                disposeWaiting.Set();
            },
        });
    }

    private sealed class Counter
    {
        public int Value;
    }

    private static SpatialReference CreateSpatialReference(int epsgCode)
    {
        var spatialReference = new SpatialReference(string.Empty);
        var importResult = spatialReference.ImportFromEPSG(epsgCode);

        Assert.Equal(0, importResult);

        spatialReference.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return spatialReference;
    }

    private static string[] GetConfiguredProjSearchPaths()
    {
        return [.. Osr.GetPROJSearchPaths().Select(Path.GetFullPath)];
    }

    private static bool ContainsLocalFile(IEnumerable<string> searchPaths, string fileName)
    {
        return searchPaths.Any(path =>
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(AppBaseDirectory, StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(fullPath, fileName));
        });
    }

    public void Dispose()
    {
        converter.Dispose();
    }
}

sealed file class BritishNationalGridLocationConverterTestCaseOrderer : Xunit.v3.ITestCaseOrderer
{
    IReadOnlyCollection<TTestCase> Xunit.v3.ITestCaseOrderer.OrderTestCases<TTestCase>(
        IReadOnlyCollection<TTestCase> testCases)
    {
        return
        [
            .. testCases
                .OrderBy(static testCase => GetPriority(GetTestMethodName(testCase)))
                .ThenBy(static testCase => GetTestMethodName(testCase), StringComparer.Ordinal),
        ];
    }

    private static int GetPriority(string testMethodName)
    {
        return string.Equals(
            testMethodName,
            nameof(BritishNationalGridLocationConverterTests.GivenConverter_WhenConversionIsAttemptedWithoutManualRuntimeBootstrap_ThenBundledOfflineRuntimeDataAndGridShiftsAreConfigured),
            StringComparison.Ordinal)
            ? 0
            : 1;
    }

    private static string GetTestMethodName<TTestCase>(TTestCase testCase)
    {
        return testCase?.GetType().GetProperty("TestMethodName")?.GetValue(testCase)?.ToString()
            ?? string.Empty;
    }
}
