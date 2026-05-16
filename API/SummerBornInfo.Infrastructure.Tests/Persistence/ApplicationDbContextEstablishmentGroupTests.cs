using SummerBornInfo.Infrastructure.Persistence;

namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextEstablishmentGroupTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewEstablishmentGroup_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();

        // Act
        dbContext.EstablishmentGroups.Add(establishmentGroup);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentGroup = dbContext.EstablishmentGroups.Find(establishmentGroup.Id);

        Assert.NotNull(savedEstablishmentGroup);
        Assert.Equivalent(establishmentGroup, savedEstablishmentGroup);
    }


    [Fact]
    public async Task GivenExistingEstablishmentGroup_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        dbContext.EstablishmentGroups.Add(establishmentGroup);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentGroupToUpdate = dbContext.EstablishmentGroups.Find(establishmentGroup.Id);
        Assert.NotNull(establishmentGroupToUpdate);
        establishmentGroupToUpdate.Code = "Update_Code";
        establishmentGroup.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentGroup = dbContext.EstablishmentGroups.Find(establishmentGroup.Id);

        Assert.NotNull(savedEstablishmentGroup);
        Assert.Equivalent(establishmentGroupToUpdate, savedEstablishmentGroup);
    }

    [Fact]
    public async Task GivenExistingEstablishmentGroup_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        ApplicationDbContext dbContext = CreateDbContext();
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        dbContext.EstablishmentGroups.Add(establishmentGroup);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();


        var establishmentGroupToUpdateOne = dbContext.EstablishmentGroups.Find(establishmentGroup.Id);
        Assert.NotNull(establishmentGroupToUpdateOne);
        establishmentGroupToUpdateOne.Code = "Code_One";

        ApplicationDbContext dbContextTwo = CreateDbContext();
        var establishmentGroupToUpdateTwo = dbContextTwo.EstablishmentGroups.Find(establishmentGroup.Id);
        Assert.NotNull(establishmentGroupToUpdateTwo);
        establishmentGroupToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedEstablishmentGroup = dbContextTwo.EstablishmentGroups.Find(establishmentGroup.Id);
        Assert.Equivalent(establishmentGroupToUpdateTwo, savedEstablishmentGroup);
    }
}
