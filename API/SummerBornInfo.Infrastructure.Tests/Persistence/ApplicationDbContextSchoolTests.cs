using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextSchoolTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewSchool_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();

        // Act
        dbContext.Schools.Add(school);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = dbContext.Schools.Find(school.Id);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(school, savedSchool);
    }

    [Fact]
    public async Task GivenNewSchoolWithOnlyRequiredFields_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        school.UKPRN = null;
        school.OpenDate = null;
        school.CloseDate = null;
        school.Address.Street = null;
        school.Address.Locality = null;
        school.Address.AddressThree = null;
        school.Address.County = null;

        // Act
        dbContext.Schools.Add(school);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = dbContext.Schools.Find(school.Id);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(school, savedSchool);
    }

    [Fact]
    public async Task GivenExistingSchool_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        dbContext.Schools.Add(school);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var schoolToUpdate = dbContext.Schools.Find(school.Id);
        Assert.NotNull(schoolToUpdate);
        schoolToUpdate.URN = 999999999;
        schoolToUpdate.UKPRN = 999999999;
        schoolToUpdate.EstablishmentNumber = 999999999;
        schoolToUpdate.Address = SchoolAddressFactory.GetSchoolAddress();
        schoolToUpdate.OpenDate = school.OpenDate?.AddDays(10);
        schoolToUpdate.CloseDate = school.CloseDate?.AddDays(10);
        schoolToUpdate.PhaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();
        schoolToUpdate.LocalAuthority = LocalAuthorityFactory.GetLocalAuthority();
        schoolToUpdate.EstablishmentType = EstablishmentTypeFactory.GetEstablishmentType();
        schoolToUpdate.EstablishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        schoolToUpdate.EstablishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();

        // Act
        dbContext.PhasesOfEducation.Add(schoolToUpdate.PhaseOfEducation);
        dbContext.LocalAuthorities.Add(schoolToUpdate.LocalAuthority);
        dbContext.EstablishmentTypes.Add(schoolToUpdate.EstablishmentType);
        dbContext.EstablishmentGroups.Add(schoolToUpdate.EstablishmentGroup);
        dbContext.PhasesOfEducation.Add(schoolToUpdate.PhaseOfEducation);
        dbContext.EstablishmentStatuses.Add(schoolToUpdate.EstablishmentStatus);
        dbContext.Schools.Update(schoolToUpdate);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = dbContext.Schools.Find(school.Id);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(schoolToUpdate, savedSchool);
    }

    [Fact]
    public async Task GivenExistingSchool_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        dbContext.Schools.Add(school);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();


        var schoolToUpdateOne = dbContext.Schools.Find(school.Id);
        Assert.NotNull(schoolToUpdateOne);
        schoolToUpdateOne.URN = 999999999;

        ApplicationDbContext dbContextTwo = CreateDbContext();
        var schoolToUpdateTwo = dbContextTwo.Schools.Find(school.Id);
        Assert.NotNull(schoolToUpdateTwo);
        schoolToUpdateTwo.URN = 999999998;
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();


        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedSchool = dbContextTwo.Schools.Find(school.Id);
        Assert.Equivalent(schoolToUpdateTwo, savedSchool);
    }
}
