using Create = SummerBornInfo.Web.Features.SchoolCommands.Create;

namespace SummerBornInfo.Web.Test.Features.SchoolCommands;
public class School_Create(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Create_SavesAuthoritySuccessfully()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            URN = 1000,
            UKPRN = 2000,
            EstablishmentNumber = 3000,
            Name = "New School",
            Address = new()
            {
                Street = "Street One",
                Locality = "Locality",
                AddressThree = "Address Three",
                Town = "Town",
                County = "County",
                PostCode = "Post Code",
            },
            OpenDate = new DateOnly(2022, 10, 21),
            CloseDate = new DateOnly(2025, 10, 21),
            PhaseOfEducationId = PostgresTestFixture.SeededData.PhaseOfEducation.Id,
            LocalAuthorityId = PostgresTestFixture.SeededData.LocalAuthority.Id,
            EstablishmentTypeId = PostgresTestFixture.SeededData.EstablishmentType.Id,
            EstablishmentGroupId = PostgresTestFixture.SeededData.EstablishmentGroup.Id,
            EstablishmentStatusId = PostgresTestFixture.SeededData.EstablishmentStatus.Id,
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var saveSchool = await dbContext.School.AsNoTracking().SingleAsync(g => g.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        saveSchool.Should().NotBeNull();
        saveSchool.Id.Should().Be(res.Id);
        saveSchool.URN.Should().Be(1000);
        saveSchool.UKPRN.Should().Be(2000);
        saveSchool.Name.Should().Be("New School");
        saveSchool.Address.Should().NotBeNull();
        saveSchool.Address.Street.Should().Be("Street One");
        saveSchool.Address.Locality.Should().Be("Locality");
        saveSchool.Address.AddressThree.Should().Be("Address Three");
        saveSchool.Address.Town.Should().Be("Town");
        saveSchool.Address.County.Should().Be("County");
        saveSchool.Address.PostCode.Should().Be("Post Code");
        saveSchool.OpenDate.Should().Be(new DateOnly(2022, 10, 21));
        saveSchool.CloseDate.Should().Be(new DateOnly(2025, 10, 21));
        saveSchool.PhaseOfEducation.Id.Should().Be(PostgresTestFixture.SeededData.PhaseOfEducation.Id);
        saveSchool.LocalAuthority.Id.Should().Be(PostgresTestFixture.SeededData.LocalAuthority.Id);
        saveSchool.EstablishmentType.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Id);
        saveSchool.EstablishmentGroup.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Id);
        saveSchool.EstablishmentStatus.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentStatus.Id);
    }

    [Fact]
    public async Task InvalidRequest_Create_ReturnsValidationErrors()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, ErrorResponse>(new()
        {
            URN = 0,
            UKPRN = 0,
            EstablishmentNumber = 0,
            Name = "",
            Address = null!,
            OpenDate = new DateOnly(),
            CloseDate = new DateOnly(),
            PhaseOfEducationId = Guid.Empty,
            LocalAuthorityId = Guid.Empty,
            EstablishmentTypeId = Guid.Empty,
            EstablishmentGroupId = Guid.Empty,
            EstablishmentStatusId = Guid.Empty,
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(11);
        res.Errors.Keys.Should()
            .Equal(
            "urn",
            "ukprn",
            "establishmentNumber",
            "name",
            "address",
            "openDate",
            "phaseOfEducationId",
            "localAuthorityId",
            "establishmentTypeId",
            "establishmentGroupId",
            "establishmentStatusId"
            );
        res.Errors["phaseOfEducationId"].Should().Equal("Phase of Education Id is required");
        res.Errors["localAuthorityId"].Should().Equal("Local Authority Id is required");
        res.Errors["establishmentTypeId"].Should().Equal("Establishment Type Id is required");
        res.Errors["establishmentGroupId"].Should().Equal("Establishment Group Id is required");
        res.Errors["establishmentStatusId"].Should().Equal("Establishment Status Id is required");
    }

    [Fact]
    public async Task LinkedEntitiesDontExist_Create_ReturnsEntityErrors()
    {
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, ErrorResponse>(new()
        {
            URN = 1000,
            UKPRN = 2000,
            EstablishmentNumber = 3000,
            Name = "New School",
            Address = new()
            {
                Street = "Street One",
                Locality = "Locality",
                AddressThree = "Address Three",
                Town = "Town",
                County = "County",
                PostCode = "PostCode",
            },
            OpenDate = new DateOnly(2022, 10, 21),
            CloseDate = new DateOnly(2025, 10, 21),
            PhaseOfEducationId = Guid.NewGuid(),
            LocalAuthorityId = Guid.NewGuid(),
            EstablishmentTypeId = Guid.NewGuid(),
            EstablishmentGroupId = Guid.NewGuid(),
            EstablishmentStatusId = Guid.NewGuid(),
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(5);
        res.Errors.Keys.Should()
            .Equal(
            "phaseOfEducationId",
            "localAuthorityId",
            "establishmentTypeId",
            "establishmentGroupId",
            "establishmentStatusId"
            );
        res.Errors["phaseOfEducationId"].Should().Equal("Phase of Education must exist");
        res.Errors["localAuthorityId"].Should().Equal("Local Authority must exist");
        res.Errors["establishmentTypeId"].Should().Equal("Establishment Type must exist");
        res.Errors["establishmentGroupId"].Should().Equal("Establishment Group must exist");
        res.Errors["establishmentStatusId"].Should().Equal("Establishment Status must exist");
    }
}
