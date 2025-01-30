using FindById = SummerBornInfo.Web.Features.SchoolQueries.FindById;
namespace SummerBornInfo.Web.Test.Features.SchoolQueries;
public class School_FindById(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_FindById_ReturnsTheSchool()
    {
        var (rsp, res) = await App.Client.GETAsync<FindById.Endpoint, FindById.Request, FindById.Response>(new()
        {
            Id = PostgresTestFixture.SeededData.School.Id
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Should().NotBeNull();
        res.Id.Should().Be(res.Id);
        res.URN.Should().Be(PostgresTestFixture.SeededData.School.URN);
        res.UKPRN.Should().Be(PostgresTestFixture.SeededData.School.UKPRN);
        res.Name.Should().Be(PostgresTestFixture.SeededData.School.Name);
        res.Address.Should().NotBeNull();
        res.Address.Street.Should().Be(PostgresTestFixture.SeededData.School.Address.Street);
        res.Address.Locality.Should().Be(PostgresTestFixture.SeededData.School.Address.Locality);
        res.Address.AddressThree.Should().Be(PostgresTestFixture.SeededData.School.Address.AddressThree);
        res.Address.Town.Should().Be(PostgresTestFixture.SeededData.School.Address.Town);
        res.Address.County.Should().Be(PostgresTestFixture.SeededData.School.Address.County);
        res.Address.PostCode.Should().Be(PostgresTestFixture.SeededData.School.Address.PostCode);
        res.OpenDate.Should().Be(PostgresTestFixture.SeededData.School.OpenDate);
        res.CloseDate.Should().Be(PostgresTestFixture.SeededData.School.CloseDate);
        res.PhaseOfEducation.Id.Should().Be(PostgresTestFixture.SeededData.PhaseOfEducation.Id);
        res.PhaseOfEducation.Code.Should().Be(PostgresTestFixture.SeededData.PhaseOfEducation.Code);
        res.PhaseOfEducation.Name.Should().Be(PostgresTestFixture.SeededData.PhaseOfEducation.Name);
        res.LocalAuthority.Id.Should().Be(PostgresTestFixture.SeededData.LocalAuthority.Id);
        res.LocalAuthority.Code.Should().Be(PostgresTestFixture.SeededData.LocalAuthority.Code);
        res.LocalAuthority.Name.Should().Be(PostgresTestFixture.SeededData.LocalAuthority.Name);
        res.EstablishmentType.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Id);
        res.EstablishmentType.Code.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Code);
        res.EstablishmentType.Name.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Name);
        res.EstablishmentGroup.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Id);
        res.EstablishmentGroup.Code.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Code);
        res.EstablishmentGroup.Name.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Name);
        res.EstablishmentStatus.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentStatus.Id);
        res.EstablishmentStatus.Code.Should().Be(PostgresTestFixture.SeededData.EstablishmentStatus.Code);
        res.EstablishmentStatus.Name.Should().Be(PostgresTestFixture.SeededData.EstablishmentStatus.Name);
    }

    [Fact]
    public async Task WrongId_FindById_ReturnsNotFound()
    {
        var rsp = await App.Client.GETAsync<FindById.Endpoint, FindById.Request>(new()
        {
            Id = Guid.NewGuid()
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvalidRequest_FindById_ReturnsErrors()
    {
        var (rsp, res) = await App.Client.GETAsync<FindById.Endpoint, FindById.Request, ErrorResponse>(new()
        {
            Id = Guid.Empty
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(1);
        res.Errors.Keys.Should().Equal("id");
    }
}
