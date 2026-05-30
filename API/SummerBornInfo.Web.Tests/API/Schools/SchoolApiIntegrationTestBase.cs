namespace SummerBornInfo.Web.Tests.API.Schools;

public abstract class SchoolApiIntegrationTestBase(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : SummerBornInfo.Web.Tests.TestFramework.WebIntegrationTestBase(testDatabaseServerFixture, testOutputHelper);
