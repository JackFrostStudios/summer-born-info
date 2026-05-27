using SummerBornInfo.Features.Schools.Queries.GetSchoolByUrn;

namespace SummerBornInfo.Features.Tests.Schools.Queries.GetSchoolByUrn;

public sealed class GetSchoolByUrnQueryHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenSchoolExistsWithRequestedUrn_WhenExecuteAsync_ThenReturnsSharedSchoolResponseShape()
    {
        var matchingSchool = SchoolFactory.GetSchool();
        matchingSchool.URN = 123456;
        matchingSchool.Name = "Northbridge Primary";

        var otherSchool = SchoolFactory.GetSchool();
        otherSchool.URN = 654321;

        await SeedSchoolsAsync(otherSchool, matchingSchool);

        var handler = new GetSchoolByUrnQueryHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new GetSchoolByUrnQuery(123456),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(matchingSchool.Id, result.Id);
        Assert.Equal(matchingSchool.URN, result.URN);
        Assert.Equal(matchingSchool.UKPRN, result.UKPRN);
        Assert.Equal(matchingSchool.EstablishmentNumber, result.EstablishmentNumber);
        Assert.Equal(matchingSchool.Name, result.Name);
        Assert.Equal(matchingSchool.OpenDate, result.OpenDate);
        Assert.Equal(matchingSchool.CloseDate, result.CloseDate);
        Assert.Equal(matchingSchool.Address.Street, result.Address.Street);
        Assert.Equal(matchingSchool.Address.Locality, result.Address.Locality);
        Assert.Equal(matchingSchool.Address.AddressThree, result.Address.AddressThree);
        Assert.Equal(matchingSchool.Address.Town, result.Address.Town);
        Assert.Equal(matchingSchool.Address.County, result.Address.County);
        Assert.Equal(matchingSchool.Address.PostCode, result.Address.PostCode);
        Assert.Equal(matchingSchool.PhaseOfEducation.Id, result.PhaseOfEducation.Id);
        Assert.Equal(matchingSchool.PhaseOfEducation.Code, result.PhaseOfEducation.Code);
        Assert.Equal(matchingSchool.PhaseOfEducation.Name, result.PhaseOfEducation.Name);
        Assert.Equal(matchingSchool.LocalAuthority.Id, result.LocalAuthority.Id);
        Assert.Equal(matchingSchool.LocalAuthority.Code, result.LocalAuthority.Code);
        Assert.Equal(matchingSchool.LocalAuthority.Name, result.LocalAuthority.Name);
        Assert.Equal(matchingSchool.EstablishmentType.Id, result.EstablishmentType.Id);
        Assert.Equal(matchingSchool.EstablishmentType.Code, result.EstablishmentType.Code);
        Assert.Equal(matchingSchool.EstablishmentType.Name, result.EstablishmentType.Name);
        Assert.Equal(matchingSchool.EstablishmentGroup.Id, result.EstablishmentGroup.Id);
        Assert.Equal(matchingSchool.EstablishmentGroup.Code, result.EstablishmentGroup.Code);
        Assert.Equal(matchingSchool.EstablishmentGroup.Name, result.EstablishmentGroup.Name);
        Assert.Equal(matchingSchool.EstablishmentStatus.Id, result.EstablishmentStatus.Id);
        Assert.Equal(matchingSchool.EstablishmentStatus.Code, result.EstablishmentStatus.Code);
        Assert.Equal(matchingSchool.EstablishmentStatus.Name, result.EstablishmentStatus.Name);
    }

    [Fact]
    public async Task GivenNoSchoolExistsWithRequestedUrn_WhenExecuteAsync_ThenReturnsNull()
    {
        await SeedSchoolsAsync(SchoolFactory.GetSchool());

        var handler = new GetSchoolByUrnQueryHandler(CreateDbContext());

        var result = await handler.ExecuteAsync(
            new GetSchoolByUrnQuery(999999),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    private async Task SeedSchoolsAsync(params School[] schools)
    {
        var dbContext = CreateDbContext();
        dbContext.Schools.AddRange(schools);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
