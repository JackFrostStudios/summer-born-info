using Microsoft.AspNetCore.Mvc;
using Get = SummerBornInfo.Web.Features.EstablishmentGroupQueries.Get;

namespace SummerBornInfo.Web.Test.Features.EstablishmentGroupQueries;
public class EstablishmentGroup_Get(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Get_ReturnsTheGroup()
    {
        var (rsp, res) = await App.Client.GETAsync<Get.Endpoint, Get.Request, Get.Response>(new()
        {
            Id = PostgresTestFixture.SeededData.EstablishmentGroup.Id
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Code.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Code);
        res.Name.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Name);
    }

    [Fact]
    public async Task WrongId_Get_ReturnsNotFound()
    {
        var rsp = await App.Client.GETAsync<Get.Endpoint, Get.Request>(new()
        {
            Id = Guid.NewGuid()
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvalidRequest_Get_ReturnsErrors()
    {
        var (rsp, res) = await App.Client.GETAsync<Get.Endpoint, Get.Request, ErrorResponse>(new()
        {
            Id = Guid.Empty
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(1);
        res.Errors.Keys.Should().Equal("id");
    }
}
