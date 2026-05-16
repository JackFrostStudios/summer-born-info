using SummerBornInfo.Infrastructure.Persistence;

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
        dbContext.LocalAuthorities.Add(localAuthority);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedLocalAuthority = dbContext.LocalAuthorities.Find(localAuthority.Id);

        Assert.NotNull(savedLocalAuthority);
        Assert.Equivalent(localAuthority, savedLocalAuthority);
    }

    [Fact]
    public async Task GivenExistingLocalAuthority_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var localAuthority = LocalAuthorityFactory.GetLocalAuthority();
        dbContext.LocalAuthorities.Add(localAuthority);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var localAuthorityToUpdate = dbContext.LocalAuthorities.Find(localAuthority.Id);
        Assert.NotNull(localAuthorityToUpdate);
        localAuthorityToUpdate.Code = "Update_Code";
        localAuthority.Name = "Update Name";

        // Act
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedLocalAuthority = dbContext.LocalAuthorities.Find(localAuthority.Id);

        Assert.NotNull(savedLocalAuthority);
        Assert.Equivalent(localAuthorityToUpdate, savedLocalAuthority);
    }

    [Fact]
    public async Task GivenExistingLocalAuthority_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var localAuthority = LocalAuthorityFactory.GetLocalAuthority();
        dbContext.LocalAuthorities.Add(localAuthority);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var localAuthorityToUpdateOne = dbContext.LocalAuthorities.Find(localAuthority.Id);
        Assert.NotNull(localAuthorityToUpdateOne);
        localAuthorityToUpdateOne.Code = "Code_One";

        var dbContextTwo = CreateDbContext();
        var localAuthorityToUpdateTwo = dbContextTwo.LocalAuthorities.Find(localAuthority.Id);
        Assert.NotNull(localAuthorityToUpdateTwo);
        localAuthorityToUpdateTwo.Code = "Code_Two";
        await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedLocalAuthority = dbContextTwo.LocalAuthorities.Find(localAuthority.Id);
        Assert.Equivalent(localAuthorityToUpdateTwo, savedLocalAuthority);
    }
}
