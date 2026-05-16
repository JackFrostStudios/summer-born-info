using Microsoft.EntityFrameworkCore;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;
using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class SchoolsImporterTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    private static readonly Guid TestRequestId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GivenCsvStream_WhenImportAsync_ThenResultIsYieldedForEachRow()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var importer = new SchoolsImporter<ApplicationDbContext>(dbContext);
        using Stream csvStream = ExampleImportFile.GetExampleImportFileContent();

        // Act
        List<SchoolImportResult> results = [];
        await foreach (SchoolImportResult result in importer.ImportAsync(TestRequestId, csvStream, TestContext.Current.CancellationToken))
        {
            results.Add(result);
        }

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(2, results[0].LineNumber);
        Assert.True(results[0].Succeeded);
        Assert.Null(results[0].ErrorMessage);
        Assert.Equal(3, results[1].LineNumber);
        Assert.True(results[1].Succeeded);
        Assert.Null(results[1].ErrorMessage);
    }

    [Fact]
    public async Task GivenCsvStream_WhenImportAsync_ThenAllSchoolsAreSaved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var importer = new SchoolsImporter<ApplicationDbContext>(dbContext);
        using Stream csvStream = ExampleImportFile.GetExampleImportFileContent();

        // Act
        var results = await importer.ImportAsync(TestRequestId, csvStream, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.True(result.Succeeded));

        var verifyDbContext = CreateDbContext();
        var schools = await verifyDbContext.Set<School>()
            .Include(s => s.LocalAuthority)
            .Include(s => s.EstablishmentType)
            .Include(s => s.EstablishmentGroup)
            .Include(s => s.EstablishmentStatus)
            .Include(s => s.PhaseOfEducation)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, schools.Count);

        var aldgateSchool = schools.SingleOrDefault(s => s.URN == 100000);
        Assert.NotNull(aldgateSchool);
        Assert.Equal(3614, aldgateSchool.EstablishmentNumber);
        Assert.Equal("The Aldgate School", aldgateSchool.Name);
        Assert.Equal(10079319, aldgateSchool.UKPRN);
        Assert.Null(aldgateSchool.OpenDate);
        Assert.Null(aldgateSchool.CloseDate);

        Assert.Equal("St James's Passage", aldgateSchool.Address.Street);
        Assert.Equal("Duke's Place", aldgateSchool.Address.Locality);
        Assert.Null(aldgateSchool.Address.AddressThree);
        Assert.Equal("London", aldgateSchool.Address.Town);
        Assert.Null(aldgateSchool.Address.County);
        Assert.Equal("EC3A 5DE", aldgateSchool.Address.PostCode);

        Assert.Equal("201", aldgateSchool.LocalAuthority.Code);
        Assert.Equal("City of London", aldgateSchool.LocalAuthority.Name);
        Assert.Equal("02", aldgateSchool.EstablishmentType.Code);
        Assert.Equal("Voluntary aided school", aldgateSchool.EstablishmentType.Name);
        Assert.Equal("4", aldgateSchool.EstablishmentGroup.Code);
        Assert.Equal("Local authority maintained schools", aldgateSchool.EstablishmentGroup.Name);
        Assert.Equal("1", aldgateSchool.EstablishmentStatus.Code);
        Assert.Equal("Open", aldgateSchool.EstablishmentStatus.Name);
        Assert.Equal("2", aldgateSchool.PhaseOfEducation.Code);
        Assert.Equal("Primary", aldgateSchool.PhaseOfEducation.Name);

        var sherborneSchool = schools.SingleOrDefault(s => s.URN == 100004);
        Assert.NotNull(sherborneSchool);
        Assert.Equal(1045, sherborneSchool.EstablishmentNumber);
        Assert.Equal("Sherborne Nursery School", sherborneSchool.Name);
        Assert.Null(sherborneSchool.UKPRN);
        Assert.Null(sherborneSchool.OpenDate);
        Assert.Equal(new DateOnly(1992, 8, 31), sherborneSchool.CloseDate);

        Assert.Equal("Priestly House", sherborneSchool.Address.Street);
        Assert.Equal("Athlone Street", sherborneSchool.Address.Locality);
        Assert.Null(sherborneSchool.Address.AddressThree);
        Assert.Equal("London", sherborneSchool.Address.Town);
        Assert.Null(sherborneSchool.Address.County);
        Assert.Equal("NW5 4LP", sherborneSchool.Address.PostCode);

        Assert.Equal("202", sherborneSchool.LocalAuthority.Code);
        Assert.Equal("Camden", sherborneSchool.LocalAuthority.Name);
        Assert.Equal("15", sherborneSchool.EstablishmentType.Code);
        Assert.Equal("Local authority nursery school", sherborneSchool.EstablishmentType.Name);
        Assert.Equal("2", sherborneSchool.EstablishmentStatus.Code);
        Assert.Equal("Closed", sherborneSchool.EstablishmentStatus.Name);
        Assert.Equal("1", sherborneSchool.PhaseOfEducation.Code);
        Assert.Equal("Nursery", sherborneSchool.PhaseOfEducation.Name);
    }

    [Fact]
    public async Task GivenSameCsvImportedTwice_WhenImportAsync_ThenSchoolsAreNotDuplicated()
    {
        // Arrange
        var dbContext1 = CreateDbContext();
        var importer1 = new SchoolsImporter<ApplicationDbContext>(dbContext1);
        using Stream csvStream1 = ExampleImportFile.GetExampleImportFileContent();

        var firstResults = await importer1.ImportAsync(TestRequestId, csvStream1, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        var dbContext2 = CreateDbContext();
        var importer2 = new SchoolsImporter<ApplicationDbContext>(dbContext2);
        using Stream csvStream2 = ExampleImportFile.GetExampleImportFileContent();

        // Act
        var secondResults = await importer2.ImportAsync(TestRequestId, csvStream2, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, firstResults.Count);
        Assert.Equal(2, secondResults.Count);
        Assert.All(firstResults, result => Assert.True(result.Succeeded));
        Assert.All(secondResults, result => Assert.True(result.Succeeded));

        var verifyDbContext = CreateDbContext();
        var schools = await verifyDbContext.Set<School>().ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, schools.Count);
    }

    [Fact]
    public async Task GivenSameCsvImportedTwice_WhenImportAsync_ThenSchoolDataRemainsUnchanged()
    {
        // Arrange
        var dbContext1 = CreateDbContext();
        var importer1 = new SchoolsImporter<ApplicationDbContext>(dbContext1);
        using Stream csvStream1 = ExampleImportFile.GetExampleImportFileContent();

        var firstResults = await importer1.ImportAsync(TestRequestId, csvStream1, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        var verifyDbContext1 = CreateDbContext();
        var schoolsAfterFirstImport = await verifyDbContext1.Set<School>()
            .Include(s => s.LocalAuthority)
            .Include(s => s.EstablishmentType)
            .Include(s => s.EstablishmentGroup)
            .Include(s => s.EstablishmentStatus)
            .Include(s => s.PhaseOfEducation)
            .OrderBy(s => s.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        var dbContext2 = CreateDbContext();
        var importer2 = new SchoolsImporter<ApplicationDbContext>(dbContext2);
        using Stream csvStream2 = ExampleImportFile.GetExampleImportFileContent();

        // Act
        var secondResults = await importer2.ImportAsync(TestRequestId, csvStream2, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, firstResults.Count);
        Assert.Equal(2, secondResults.Count);

        var verifyDbContext2 = CreateDbContext();
        var schoolsAfterSecondImport = await verifyDbContext2.Set<School>()
            .Include(s => s.LocalAuthority)
            .Include(s => s.EstablishmentType)
            .Include(s => s.EstablishmentGroup)
            .Include(s => s.EstablishmentStatus)
            .Include(s => s.PhaseOfEducation)
            .OrderBy(s => s.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(schoolsAfterFirstImport.Count, schoolsAfterSecondImport.Count);

        for (int i = 0; i < schoolsAfterFirstImport.Count; i++)
        {
            var first = schoolsAfterFirstImport[i];
            var second = schoolsAfterSecondImport[i];

            Assert.Equal(first.URN, second.URN);
            Assert.Equal(first.EstablishmentNumber, second.EstablishmentNumber);
            Assert.Equal(first.Name, second.Name);
            Assert.Equal(first.OpenDate, second.OpenDate);
            Assert.Equal(first.CloseDate, second.CloseDate);
            Assert.Equal(first.UKPRN, second.UKPRN);

            Assert.Equal(first.Address.Street, second.Address.Street);
            Assert.Equal(first.Address.Locality, second.Address.Locality);
            Assert.Equal(first.Address.AddressThree, second.Address.AddressThree);
            Assert.Equal(first.Address.Town, second.Address.Town);
            Assert.Equal(first.Address.County, second.Address.County);
            Assert.Equal(first.Address.PostCode, second.Address.PostCode);

            Assert.Equal(first.LocalAuthority.Code, second.LocalAuthority.Code);
            Assert.Equal(first.EstablishmentType.Code, second.EstablishmentType.Code);
            Assert.Equal(first.EstablishmentGroup.Code, second.EstablishmentGroup.Code);
            Assert.Equal(first.EstablishmentStatus.Code, second.EstablishmentStatus.Code);
            Assert.Equal(first.PhaseOfEducation.Code, second.PhaseOfEducation.Code);
        }
    }

    [Fact]
    public async Task GivenCsvStream_WhenImportAsync_ThenLookupEntitiesAreNotDuplicated()
    {
        // Arrange
        var dbContext1 = CreateDbContext();
        var importer1 = new SchoolsImporter<ApplicationDbContext>(dbContext1);
        using Stream csvStream1 = ExampleImportFile.GetExampleImportFileContent();

        var firstResults = await importer1.ImportAsync(TestRequestId, csvStream1, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        var dbContext2 = CreateDbContext();
        var importer2 = new SchoolsImporter<ApplicationDbContext>(dbContext2);
        using Stream csvStream2 = ExampleImportFile.GetExampleImportFileContent();

        // Act
        var secondResults = await importer2.ImportAsync(TestRequestId, csvStream2, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, firstResults.Count);
        Assert.Equal(2, secondResults.Count);

        var verifyDbContext = CreateDbContext();

        var localAuthorities = await verifyDbContext.Set<LocalAuthority>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, localAuthorities.Count);

        var establishmentTypes = await verifyDbContext.Set<EstablishmentType>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, establishmentTypes.Count);

        var establishmentGroups = await verifyDbContext.Set<EstablishmentGroup>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Single(establishmentGroups);

        var establishmentStatuses = await verifyDbContext.Set<EstablishmentStatus>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, establishmentStatuses.Count);

        var phasesOfEducation = await verifyDbContext.Set<PhaseOfEducation>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, phasesOfEducation.Count);
    }

    [Fact]
    public async Task GivenCsvStreamWithInvalidRow_WhenImportAsync_ThenFailedResultIsYieldedAndImportContinues()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var importer = new SchoolsImporter<ApplicationDbContext>(dbContext);
        using MemoryStream csvStream = CreateCsvStream(
            "\"URN\",\"EstablishmentNumber\",\"EstablishmentName\",\"LA (code)\",\"LA (name)\",\"TypeOfEstablishment (code)\",\"TypeOfEstablishment (name)\",\"EstablishmentTypeGroup (code)\",\"EstablishmentTypeGroup (name)\",\"EstablishmentStatus (code)\",\"EstablishmentStatus (name)\",\"PhaseOfEducation (code)\",\"PhaseOfEducation (name)\",\"OpenDate\",\"CloseDate\",\"UKPRN\",\"Street\",\"Locality\",\"Address3\",\"Town\",\"County (name)\",\"Postcode\"",
            "\"100000\",\"3614\",\"The Aldgate School\",\"201\",\"City of London\",\"02\",\"Voluntary aided school\",\"4\",\"Local authority maintained schools\",\"1\",\"Open\",\"2\",\"Primary\",\"\",\"\",\"10079319\",\"St James's Passage\",\"Duke's Place\",\"\",\"London\",\"\",\"EC3A 5DE\"",
            "\"INVALID\",\"1045\",\"Broken School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"",
            "\"100004\",\"1045\",\"Sherborne Nursery School\",\"202\",\"Camden\",\"15\",\"Local authority nursery school\",\"4\",\"Local authority maintained schools\",\"2\",\"Closed\",\"1\",\"Nursery\",\"\",\"31-08-1992\",\"\",\"Priestly House\",\"Athlone Street\",\"\",\"London\",\"\",\"NW5 4LP\"");

        // Act
        var results = await importer.ImportAsync(TestRequestId, csvStream, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(3, results.Count);

        Assert.True(results[0].Succeeded);
        Assert.Equal(2, results[0].LineNumber);
        Assert.Null(results[0].ErrorMessage);

        Assert.False(results[1].Succeeded);
        Assert.Equal(3, results[1].LineNumber);
        Assert.NotNull(results[1].ErrorMessage);

        Assert.True(results[2].Succeeded);
        Assert.Equal(4, results[2].LineNumber);
        Assert.Null(results[2].ErrorMessage);

        var verifyDbContext = CreateDbContext();
        var schools = await verifyDbContext.Set<School>()
            .OrderBy(x => x.URN)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, schools.Count);
        Assert.Equal([100000, 100004], schools.Select(x => x.URN).ToArray());
    }

    private static MemoryStream CreateCsvStream(params string[] lines) =>
        new(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
}
