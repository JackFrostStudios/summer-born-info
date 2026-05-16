namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextEstablishmentGroupTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewEstablishmentGroup_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();

        // Act
        dbContext.EstablishmentGroups.Add(establishmentGroup);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentGroup = await dbContext.EstablishmentGroups.FindAsync([establishmentGroup.Id], cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedEstablishmentGroup);
        Assert.Equivalent(establishmentGroup, savedEstablishmentGroup);
    }

    [Fact]
    public async Task GivenExistingEstablishmentGroup_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        dbContext.EstablishmentGroups.Add(establishmentGroup);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentGroupToUpdate = await dbContext.EstablishmentGroups.FindAsync([establishmentGroup.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentGroupToUpdate);
        establishmentGroupToUpdate.Code = "Update_Code";
        establishmentGroup.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentGroup = await dbContext.EstablishmentGroups.FindAsync([establishmentGroup.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedEstablishmentGroup);
        Assert.Equivalent(establishmentGroupToUpdate, savedEstablishmentGroup);
    }

    [Fact]
    public async Task GivenExistingEstablishmentGroup_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        dbContext.EstablishmentGroups.Add(establishmentGroup);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentGroupToUpdateOne = await dbContext.EstablishmentGroups.FindAsync([establishmentGroup.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentGroupToUpdateOne);
        establishmentGroupToUpdateOne.Code = "Code_One";

        var dbContextTwo = CreateDbContext();
        var establishmentGroupToUpdateTwo = await dbContextTwo.EstablishmentGroups.FindAsync([establishmentGroup.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentGroupToUpdateTwo);
        establishmentGroupToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedEstablishmentGroup = await dbContextTwo.EstablishmentGroups.FindAsync([establishmentGroup.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(establishmentGroupToUpdateTwo, savedEstablishmentGroup);
    }
}
