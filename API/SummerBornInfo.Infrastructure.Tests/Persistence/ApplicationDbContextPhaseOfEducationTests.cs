using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextPhaseOfEducationTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewPhaseOfEducation_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();

        // Act
        dbContext.PhasesOfEducation.Add(phaseOfEducation);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedPhaseOfEducation = dbContext.PhasesOfEducation.Find(phaseOfEducation.Id);

        Assert.NotNull(savedPhaseOfEducation);
        Assert.Equivalent(phaseOfEducation, savedPhaseOfEducation);
    }


    [Fact]
    public async Task GivenExistingPhaseOfEducation_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();
        dbContext.PhasesOfEducation.Add(phaseOfEducation);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var phaseOfEducationToUpdate = dbContext.PhasesOfEducation.Find(phaseOfEducation.Id);
        Assert.NotNull(phaseOfEducationToUpdate);
        phaseOfEducationToUpdate.Code = "Update_Code";
        phaseOfEducation.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedPhaseOfEducation = dbContext.PhasesOfEducation.Find(phaseOfEducation.Id);

        Assert.NotNull(savedPhaseOfEducation);
        Assert.Equivalent(phaseOfEducationToUpdate, savedPhaseOfEducation);
    }

    [Fact]
    public async Task GivenExistingPhaseOfEducation_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();
        dbContext.PhasesOfEducation.Add(phaseOfEducation);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();


        var phaseOfEducationToUpdateOne = dbContext.PhasesOfEducation.Find(phaseOfEducation.Id);
        Assert.NotNull(phaseOfEducationToUpdateOne);
        phaseOfEducationToUpdateOne.Code = "Code_One";

        ApplicationDbContext dbContextTwo = CreateDbContext();
        var phaseOfEducationToUpdateTwo = dbContextTwo.PhasesOfEducation.Find(phaseOfEducation.Id);
        Assert.NotNull(phaseOfEducationToUpdateTwo);
        phaseOfEducationToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedPhaseOfEducation = dbContextTwo.PhasesOfEducation.Find(phaseOfEducation.Id);
        Assert.Equivalent(phaseOfEducationToUpdateTwo, savedPhaseOfEducation);
    }
}
