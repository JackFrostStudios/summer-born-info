﻿using Create = SummerBornInfo.Web.Features.EstablishmentTypeCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.EstablishmentTypeCommands;
public class EstablishmentType_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesTypeSuccessfully()
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
        var savedGroup = await dbContext.EstablishmentType.AsNoTracking().SingleAsync(et => et.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedGroup.Should().NotBeNull();
        savedGroup.Id.Should().Be(res.Id);
        savedGroup.Code.Should().Be(300);
        savedGroup.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ExistingTypeByCode_Upsert_SavesTypeSuccessfully()
    {
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededType = new EstablishmentType{ Code = 400, Name = "Original Type" };
        var x = seedingContext.Add(seededType);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            Code = 400,
            Name = "Updated Type"
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededType.Id);
        res.Code.Should().Be(400);
        res.Name.Should().Be("Updated Type");

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var savedGroup = await dbContext.EstablishmentType.AsNoTracking().SingleAsync(et => et.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedGroup.Should().NotBeNull();
        savedGroup.Id.Should().Be(res.Id);
        savedGroup.Code.Should().Be(400);
        savedGroup.Name.Should().Be("Updated Type");
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
