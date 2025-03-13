using Create = SummerBornInfo.Web.Features.LocalAuthorityCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.LocalAuthorityCommands;
public class LocalAuthority_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesAuthoritySuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedCode();
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = code,
            Name = "Test"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Code.Should().Be(code);
        res.Name.Should().Be("Test");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedAuthority = await dbContext.LocalAuthority.AsNoTracking().SingleAsync(a => a.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedAuthority.Should().NotBeNull();
        savedAuthority.Id.Should().Be(res.Id);
        savedAuthority.Code.Should().Be(code);
        savedAuthority.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingAuthorityByCode_Upsert_SavesAuthoritySuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedCode();
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededAuthority = new LocalAuthority { Code = code, Name = "Original Authority" };
        seedingContext.Add(seededAuthority);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = code,
            Name = "Updated Authority"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededAuthority.Id);
        res.Code.Should().Be(code);
        res.Name.Should().Be("Updated Authority");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedAuthority = await dbContext.LocalAuthority.AsNoTracking().SingleAsync(a => a.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedAuthority.Should().NotBeNull();
        savedAuthority.Id.Should().Be(res.Id);
        savedAuthority.Code.Should().Be(code);
        savedAuthority.Name.Should().Be("Updated Authority");
    }

    [Fact]
    public async Task InvalidRequest_Upsert_ReturnsErrors()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, ErrorResponse>(new()
        {
            Code = "",
            Name = "",
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(2);
        res.Errors.Keys.Should().Equal("code", "name");
    }
}
