using Create = SummerBornInfo.Web.Features.EstablishmentTypeCommands.Create;

namespace SummerBornInfo.Web.Test.Features.EstablishmentTypeCommands;
public class EstablishmentType_Create(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Create_SavesTypeSuccessfully()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = 300,
            Name = "Test"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Code.Should().Be(300);
        res.Name.Should().Be("Test");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedGroup = await dbContext.EstablishmentType.AsNoTracking().SingleAsync(g => g.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedGroup.Should().NotBeNull();
        savedGroup.Id.Should().Be(res.Id);
        savedGroup.Code.Should().Be(300);
        savedGroup.Name.Should().Be("Test");
    }

    [Fact]
    public async Task InvalidRequest_Create_ReturnsErrors()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, ErrorResponse>(new()
        {
            Code = 0,
            Name = "",
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(2);
        res.Errors.Keys.Should().Equal("code", "name");
    }
}
