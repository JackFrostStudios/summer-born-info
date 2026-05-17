namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextLocalAuthorityTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenNewLocalAuthority_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var localAuthority = LocalAuthorityFactory.GetLocalAuthority();

        // Act
        _ = dbContext.LocalAuthorities.Add(localAuthority);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedLocalAuthority = await dbContext.LocalAuthorities.FindAsync([localAuthority.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedLocalAuthority);
        Assert.Equivalent(localAuthority, savedLocalAuthority);
    }

    [Fact]
    public async Task GivenExistingLocalAuthority_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var localAuthority = LocalAuthorityFactory.GetLocalAuthority();
        _ = dbContext.LocalAuthorities.Add(localAuthority);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var localAuthorityToUpdate = await dbContext.LocalAuthorities.FindAsync([localAuthority.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(localAuthorityToUpdate);
        localAuthorityToUpdate.Code = "Update_Code";
        localAuthority.Name = "Update Name";

        // Act
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedLocalAuthority = await dbContext.LocalAuthorities.FindAsync([localAuthority.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedLocalAuthority);
        Assert.Equivalent(localAuthorityToUpdate, savedLocalAuthority);
    }

    [Fact]
    public async Task GivenExistingLocalAuthority_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var localAuthority = LocalAuthorityFactory.GetLocalAuthority();
        _ = dbContext.LocalAuthorities.Add(localAuthority);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var localAuthorityToUpdateOne = await dbContext.LocalAuthorities.FindAsync([localAuthority.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(localAuthorityToUpdateOne);
        localAuthorityToUpdateOne.Code = "Code_One";

        var dbContextTwo = CreateDbContext();
        var localAuthorityToUpdateTwo = await dbContextTwo.LocalAuthorities.FindAsync([localAuthority.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(localAuthorityToUpdateTwo);
        localAuthorityToUpdateTwo.Code = "Code_Two";
        _ = await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        _ = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedLocalAuthority = await dbContextTwo.LocalAuthorities.FindAsync([localAuthority.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(localAuthorityToUpdateTwo, savedLocalAuthority);
    }
}
