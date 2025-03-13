using SummerBornInfo.Web.Features.EstablishmentGroupCommands.Upsert;
using Upsert = SummerBornInfo.Web.Features.EstablishmentGroupCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.EstablishmentGroupCommands;
public class EstablishmentGroup_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesGroupSuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedCode();
        var (rsp, res) = await App.Client.POSTAsync<Endpoint, Upsert.Request, Upsert.Response>(new()
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
        var savedGroup = await dbContext.EstablishmentGroup.AsNoTracking().SingleAsync(g => g.Id == res.Id, TestContext.Current.CancellationToken);
        savedGroup.Should().NotBeNull();
        savedGroup.Id.Should().Be(res.Id);
        savedGroup.Code.Should().Be(code);
        savedGroup.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingGroupByCode_Upsert_UpdatesGroupSuccessfully()
    {
        var code = PostgresTestFixture.SeededData.NextSeedCode();
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededGroup = new EstablishmentGroup { Code = code, Name = "Original Group" };
        seedingContext.Add(seededGroup);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Endpoint, Upsert.Request, Upsert.Response>(new()
        {
            Code = code,
            Name = "Updated Group"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededGroup.Id);
        res.Code.Should().Be(code);
        res.Name.Should().Be("Updated Group");

        using var scope = App.Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedGroup = await dbContext.EstablishmentGroup.AsNoTracking().SingleAsync(g => g.Id == res.Id, TestContext.Current.CancellationToken);
        savedGroup.Should().NotBeNull();
        savedGroup.Id.Should().Be(res.Id);
        savedGroup.Code.Should().Be(code);
        savedGroup.Name.Should().Be("Updated Group");
    }

    [Fact]
    public async Task InvalidRequest_Upsert_ReturnsErrors()
    {
        var (rsp, res) = await App.Client.POSTAsync<Endpoint, Upsert.Request, ErrorResponse>(new()
        {
            Code = "",
            Name = "",
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(2);
        res.Errors.Keys.Should().Equal("code", "name");
    }
}
