namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextEstablishmentTypeTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewEstablishmentType_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentType = EstablishmentTypeFactory.GetEstablishmentType();

        // Act
        dbContext.EstablishmentTypes.Add(establishmentType);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentType = await dbContext.EstablishmentTypes.FindAsync([establishmentType.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedEstablishmentType);
        Assert.Equivalent(establishmentType, savedEstablishmentType);
    }

    [Fact]
    public async Task GivenExistingEstablishmentType_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentType = EstablishmentTypeFactory.GetEstablishmentType();
        dbContext.EstablishmentTypes.Add(establishmentType);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentTypeToUpdate = await dbContext.EstablishmentTypes.FindAsync([establishmentType.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentTypeToUpdate);
        establishmentTypeToUpdate.Code = "Update_Code";
        establishmentType.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentType = await dbContext.EstablishmentTypes.FindAsync([establishmentType.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedEstablishmentType);
        Assert.Equivalent(establishmentTypeToUpdate, savedEstablishmentType);
    }

    [Fact]
    public async Task GivenExistingEstablishmentType_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentType = EstablishmentTypeFactory.GetEstablishmentType();
        dbContext.EstablishmentTypes.Add(establishmentType);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentTypeToUpdateOne = await dbContext.EstablishmentTypes.FindAsync([establishmentType.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentTypeToUpdateOne);
        establishmentTypeToUpdateOne.Code = "Code_One";

        var dbContextTwo = CreateDbContext();
        var establishmentTypeToUpdateTwo = await dbContextTwo.EstablishmentTypes.FindAsync([establishmentType.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentTypeToUpdateTwo);
        establishmentTypeToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedEstablishmentType = await dbContextTwo.EstablishmentTypes.FindAsync([establishmentType.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(establishmentTypeToUpdateTwo, savedEstablishmentType);
    }
}
