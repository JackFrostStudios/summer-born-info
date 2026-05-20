namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextSchoolTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewSchool_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();

        // Act
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(school, savedSchool);
    }

    [Fact]
    public async Task GivenNewSchoolWithOnlyRequiredFields_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        school.UKPRN = null;
        school.OpenDate = null;
        school.CloseDate = null;
        school.Address.Street = null;
        school.Address.Locality = null;
        school.Address.AddressThree = null;
        school.Address.County = null;

        // Act
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(school, savedSchool);
    }

    [Fact]
    public async Task GivenExistingSchool_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var schoolToUpdate = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
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
        _ = dbContext.PhasesOfEducation.Add(schoolToUpdate.PhaseOfEducation);
        _ = dbContext.LocalAuthorities.Add(schoolToUpdate.LocalAuthority);
        _ = dbContext.EstablishmentTypes.Add(schoolToUpdate.EstablishmentType);
        _ = dbContext.EstablishmentGroups.Add(schoolToUpdate.EstablishmentGroup);
        _ = dbContext.PhasesOfEducation.Add(schoolToUpdate.PhaseOfEducation);
        _ = dbContext.EstablishmentStatuses.Add(schoolToUpdate.EstablishmentStatus);
        _ = dbContext.Schools.Update(schoolToUpdate);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(schoolToUpdate, savedSchool);
    }

    [Fact]
    public async Task GivenExistingSchool_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var schoolToUpdateOne = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(schoolToUpdateOne);
        schoolToUpdateOne.URN = 999999999;

        var dbContextTwo = CreateDbContext();
        var schoolToUpdateTwo = await dbContextTwo.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(schoolToUpdateTwo);
        schoolToUpdateTwo.URN = 999999998;
        _ = await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        _ = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedSchool = await dbContextTwo.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(schoolToUpdateTwo, savedSchool);
    }
}
