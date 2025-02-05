using Create = SummerBornInfo.Web.Features.LocalAuthorityCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.LocalAuthorityCommands;
public class LocalAuthority_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesAuthoritySuccessfully()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = 400,
            Name = "Test"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Code.Should().Be(400);
        res.Name.Should().Be("Test");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedAuthority = await dbContext.LocalAuthority.AsNoTracking().SingleAsync(a => a.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedAuthority.Should().NotBeNull();
        savedAuthority.Id.Should().Be(res.Id);
        savedAuthority.Code.Should().Be(400);
        savedAuthority.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingAuthorityByCode_Upsert_SavesAuthoritySuccessfully()
    {
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededAuthority = new LocalAuthority { Code = 500, Name = "Original Authority" };
        seedingContext.Add(seededAuthority);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = 500,
            Name = "Updated Authority"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededAuthority.Id);
        res.Code.Should().Be(500);
        res.Name.Should().Be("Updated Authority");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedAuthority = await dbContext.LocalAuthority.AsNoTracking().SingleAsync(a => a.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedAuthority.Should().NotBeNull();
        savedAuthority.Id.Should().Be(res.Id);
        savedAuthority.Code.Should().Be(500);
        savedAuthority.Name.Should().Be("Updated Authority");
    }

    [Fact]
    public async Task InvalidRequest_Upsert_ReturnsErrors()
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
