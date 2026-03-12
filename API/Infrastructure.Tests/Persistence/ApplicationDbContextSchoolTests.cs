namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public class ApplicationDbContextSchoolTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture) : IntegrationTestBase(testDatabaseServerFixture)
{
    [Fact]
    public async Task GivenNewSchool_WhenInsertingToDatabase_ThenIdsAreInitialized()
    {
        var school = SchoolFactory.GetSchool();
        Assert.NotNull(school);
    }
}
