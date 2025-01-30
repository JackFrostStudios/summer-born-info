using FindById = SummerBornInfo.Web.Features.EstablishmentTypeQueries.FindById;
namespace SummerBornInfo.Web.Test.Features.EstablishmentTypeQueries;
public class EstablishmentType_FindById(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_FindById_ReturnsTheType()
    {
        var (rsp, res) = await App.Client.GETAsync<FindById.Endpoint, FindById.Request, FindById.Response>(new()
        {
            Id = PostgresTestFixture.SeededData.EstablishmentType.Id
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Code.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Code);
        res.Name.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Name);
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
