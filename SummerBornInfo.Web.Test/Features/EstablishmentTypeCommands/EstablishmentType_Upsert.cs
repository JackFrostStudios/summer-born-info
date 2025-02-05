using Create = SummerBornInfo.Web.Features.EstablishmentTypeCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.EstablishmentTypeCommands;
public class EstablishmentType_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesTypeSuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedNumber();
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
        var savedType = await dbContext.EstablishmentType.AsNoTracking().SingleAsync(et => et.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedType.Should().NotBeNull();
        savedType.Id.Should().Be(res.Id);
        savedType.Code.Should().Be(code);
        savedType.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingTypeByCode_Upsert_SavesTypeSuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedNumber();
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededType = new EstablishmentType{ Code = code, Name = "Original Type" };
        seedingContext.Add(seededType);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = code,
            Name = "Updated Type"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededType.Id);
        res.Code.Should().Be(code);
        res.Name.Should().Be("Updated Type");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedType = await dbContext.EstablishmentType.AsNoTracking().SingleAsync(et => et.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedType.Should().NotBeNull();
        savedType.Id.Should().Be(res.Id);
        savedType.Code.Should().Be(code);
        savedType.Name.Should().Be("Updated Type");
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
