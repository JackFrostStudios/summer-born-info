using Microsoft.EntityFrameworkCore;
using SummerBornInfo.Features.Schools.Commands.ProcessImportFile.FileProcessing;
using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Features.Tests.Schools.Commands.ProcessImportFile.FileProcessing;

public sealed class SchoolsImporterTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenCsvStream_WhenImportAsync_ThenAllSchoolsAreSaved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var importer = new SchoolsImporter<ApplicationDbContext>(dbContext);
        using var csvStream = ExampleImportFile.GetExampleImportFileContent();

        // Act
        var processedCount = await importer.ImportAsync(csvStream, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, processedCount);

        var verifyDbContext = CreateDbContext();
        var schools = await verifyDbContext.Set<School>()
            .Include(s => s.LocalAuthority)
            .Include(s => s.EstablishmentType)
            .Include(s => s.EstablishmentGroup)
            .Include(s => s.EstablishmentStatus)
            .Include(s => s.PhaseOfEducation)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, schools.Count);

        // Verify first school (The Aldgate School)
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

        // Verify second school (Sherborne Nursery School)
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
        using var csvStream1 = ExampleImportFile.GetExampleImportFileContent();

        // First import
        await importer1.ImportAsync(csvStream1, TestContext.Current.CancellationToken);

        var dbContext2 = CreateDbContext();
        var importer2 = new SchoolsImporter<ApplicationDbContext>(dbContext2);
        using var csvStream2 = ExampleImportFile.GetExampleImportFileContent();

        // Act - Second import
        var processedCount = await importer2.ImportAsync(csvStream2, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, processedCount);

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
        using var csvStream1 = ExampleImportFile.GetExampleImportFileContent();

        await importer1.ImportAsync(csvStream1, TestContext.Current.CancellationToken);

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
        using var csvStream2 = ExampleImportFile.GetExampleImportFileContent();

        // Act
        await importer2.ImportAsync(csvStream2, TestContext.Current.CancellationToken);

        // Assert
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
        using var csvStream1 = ExampleImportFile.GetExampleImportFileContent();

        await importer1.ImportAsync(csvStream1, TestContext.Current.CancellationToken);

        var dbContext2 = CreateDbContext();
        var importer2 = new SchoolsImporter<ApplicationDbContext>(dbContext2);
        using var csvStream2 = ExampleImportFile.GetExampleImportFileContent();

        // Act
        await importer2.ImportAsync(csvStream2, TestContext.Current.CancellationToken);

        // Assert
        var verifyDbContext = CreateDbContext();

        var localAuthorities = await verifyDbContext.Set<LocalAuthority>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, localAuthorities.Count); // City of London and Camden

        var establishmentTypes = await verifyDbContext.Set<EstablishmentType>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, establishmentTypes.Count); // Voluntary aided school and Local authority nursery school

        var establishmentGroups = await verifyDbContext.Set<EstablishmentGroup>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Single(establishmentGroups); // Both are Local authority maintained schools (code 4)

        var establishmentStatuses = await verifyDbContext.Set<EstablishmentStatus>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, establishmentStatuses.Count); // Open and Closed

        var phasesOfEducation = await verifyDbContext.Set<PhaseOfEducation>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, phasesOfEducation.Count); // Primary and Nursery
    }
}
