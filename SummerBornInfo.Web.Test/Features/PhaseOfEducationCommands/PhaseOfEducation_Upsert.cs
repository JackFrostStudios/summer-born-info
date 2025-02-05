using Create = SummerBornInfo.Web.Features.PhaseOfEducationCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.PhaseOfEducationCommands;
public class PhaseOfEducation_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesPhaseSuccessfully()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = 500,
            Name = "Test"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Code.Should().Be(500);
        res.Name.Should().Be("Test");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedPhase = await dbContext.PhaseOfEducation.AsNoTracking().SingleAsync(p => p.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedPhase.Should().NotBeNull();
        savedPhase.Id.Should().Be(res.Id);
        savedPhase.Code.Should().Be(500);
        savedPhase.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingPhaseByCode_Upsert_UpdatesPhaseSuccessfully()
    {
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededPhase = new PhaseOfEducation { Code = 600, Name = "Original Phase" };
        seedingContext.Add(seededPhase);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = 600,
            Name = "Updated Phase"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededPhase.Id);
        res.Code.Should().Be(600);
        res.Name.Should().Be("Updated Phase");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedPhase = await dbContext.PhaseOfEducation.AsNoTracking().SingleAsync(p => p.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedPhase.Should().NotBeNull();
        savedPhase.Id.Should().Be(res.Id);
        savedPhase.Code.Should().Be(600);
        savedPhase.Name.Should().Be("Updated Phase");
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
