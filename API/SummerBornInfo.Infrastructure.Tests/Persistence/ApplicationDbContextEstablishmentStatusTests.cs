namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextEstablishmentStatusTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewEstablishmentStatus_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();

        // Act
        dbContext.EstablishmentStatuses.Add(establishmentStatus);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentStatus = await dbContext.EstablishmentStatuses.FindAsync([establishmentStatus.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedEstablishmentStatus);
        Assert.Equivalent(establishmentStatus, savedEstablishmentStatus);
    }

    [Fact]
    public async Task GivenExistingEstablishmentStatus_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();
        dbContext.EstablishmentStatuses.Add(establishmentStatus);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentStatusToUpdate = await dbContext.EstablishmentStatuses.FindAsync([establishmentStatus.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentStatusToUpdate);
        establishmentStatusToUpdate.Code = "Update_Code";
        establishmentStatus.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedEstablishmentStatus = await dbContext.EstablishmentStatuses.FindAsync([establishmentStatus.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedEstablishmentStatus);
        Assert.Equivalent(establishmentStatusToUpdate, savedEstablishmentStatus);
    }

    [Fact]
    public async Task GivenExistingEstablishmentStatus_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var establishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();
        dbContext.EstablishmentStatuses.Add(establishmentStatus);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var establishmentStatusToUpdateOne = await dbContext.EstablishmentStatuses.FindAsync([establishmentStatus.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentStatusToUpdateOne);
        establishmentStatusToUpdateOne.Code = "Code_One";

        var dbContextTwo = CreateDbContext();
        var establishmentStatusToUpdateTwo = await dbContextTwo.EstablishmentStatuses.FindAsync([establishmentStatus.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(establishmentStatusToUpdateTwo);
        establishmentStatusToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedEstablishmentStatus = await dbContextTwo.EstablishmentStatuses.FindAsync([establishmentStatus.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(establishmentStatusToUpdateTwo, savedEstablishmentStatus);
    }
}
