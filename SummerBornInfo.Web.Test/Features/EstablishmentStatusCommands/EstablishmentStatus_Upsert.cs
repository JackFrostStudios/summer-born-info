using Create = SummerBornInfo.Web.Features.EstablishmentStatusCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.EstablishmentStatusCommands;
public class EstablishmentStatus_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesStatusSuccessfully()
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
        var savedStatus = await dbContext.EstablishmentStatus.AsNoTracking().SingleAsync(s => s.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedStatus.Should().NotBeNull();
        savedStatus.Id.Should().Be(res.Id);
        savedStatus.Code.Should().Be(code);
        savedStatus.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingStatusByCode_Upsert_UpdatesStatusSuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedNumber();
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededStatus = new EstablishmentStatus { Code = code, Name = "Original Status" };
        seedingContext.Add(seededStatus);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = code,
            Name = "Updated Status"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededStatus.Id);
        res.Code.Should().Be(code);
        res.Name.Should().Be("Updated Status");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedStatus = await dbContext.EstablishmentStatus.AsNoTracking().SingleAsync(s => s.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedStatus.Should().NotBeNull();
        savedStatus.Id.Should().Be(res.Id);
        savedStatus.Code.Should().Be(code);
        savedStatus.Name.Should().Be("Updated Status");
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
