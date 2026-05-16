using Bogus.DataSets;
using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextPhaseOfEducationTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewPhaseOfEducation_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();

        // Act
        dbContext.PhasesOfEducation.Add(phaseOfEducation);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedPhaseOfEducation = await dbContext.PhasesOfEducation.FindAsync([phaseOfEducation.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedPhaseOfEducation);
        Assert.Equivalent(phaseOfEducation, savedPhaseOfEducation);
    }

    [Fact]
    public async Task GivenExistingPhaseOfEducation_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();
        dbContext.PhasesOfEducation.Add(phaseOfEducation);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var phaseOfEducationToUpdate = await dbContext.PhasesOfEducation.FindAsync([phaseOfEducation.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(phaseOfEducationToUpdate);
        phaseOfEducationToUpdate.Code = "Update_Code";
        phaseOfEducation.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedPhaseOfEducation = await dbContext.PhasesOfEducation.FindAsync([phaseOfEducation.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedPhaseOfEducation);
        Assert.Equivalent(phaseOfEducationToUpdate, savedPhaseOfEducation);
    }

    [Fact]
    public async Task GivenExistingPhaseOfEducation_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var phaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();
        dbContext.PhasesOfEducation.Add(phaseOfEducation);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var phaseOfEducationToUpdateOne = await dbContext.PhasesOfEducation.FindAsync([phaseOfEducation.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(phaseOfEducationToUpdateOne);
        phaseOfEducationToUpdateOne.Code = "Code_One";

        var dbContextTwo = CreateDbContext();
        var phaseOfEducationToUpdateTwo = await dbContextTwo.PhasesOfEducation.FindAsync([phaseOfEducation.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(phaseOfEducationToUpdateTwo);
        phaseOfEducationToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedPhaseOfEducation = await dbContextTwo.PhasesOfEducation.FindAsync([phaseOfEducation.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(phaseOfEducationToUpdateTwo, savedPhaseOfEducation);
    }
}
