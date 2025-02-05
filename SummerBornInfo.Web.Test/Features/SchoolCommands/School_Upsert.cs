using Create = SummerBornInfo.Web.Features.SchoolCommands.Upsert;

namespace SummerBornInfo.Web.Test.Features.SchoolCommands;
public class School_Upsert(PostgresTestFixture App) : BaseIntegrationTest
{
    [Fact]
    public async Task ValidRequest_Upsert_SavesSchoolSuccessfully()
    {
        var URN = PostgresTestFixture.SeededData.NextSeedNumber();
        var UKPRN = PostgresTestFixture.SeededData.NextSeedNumber();
        var EstablishmentNumber = PostgresTestFixture.SeededData.NextSeedNumber();
        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            URN = URN,
            UKPRN = UKPRN,
            EstablishmentNumber = EstablishmentNumber,
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
        var savedSchool = await dbContext.School.AsNoTracking().SingleAsync(g => g.Id == res.Id, cancellationToken: TestContext.Current.CancellationToken);
        savedSchool.Should().NotBeNull();
        savedSchool.Id.Should().Be(res.Id);
        savedSchool.URN.Should().Be(URN);
        savedSchool.UKPRN.Should().Be(UKPRN);
        savedSchool.EstablishmentNumber.Should().Be(EstablishmentNumber);
        savedSchool.Name.Should().Be("New School");
        savedSchool.Address.Should().NotBeNull();
        savedSchool.Address.Street.Should().Be("Street One");
        savedSchool.Address.Locality.Should().Be("Locality");
        savedSchool.Address.AddressThree.Should().Be("Address Three");
        savedSchool.Address.Town.Should().Be("Town");
        savedSchool.Address.County.Should().Be("County");
        savedSchool.Address.PostCode.Should().Be("Post Code");
        savedSchool.OpenDate.Should().Be(new DateOnly(2022, 10, 21));
        savedSchool.CloseDate.Should().Be(new DateOnly(2025, 10, 21));
        savedSchool.PhaseOfEducation.Id.Should().Be(PostgresTestFixture.SeededData.PhaseOfEducation.Id);
        savedSchool.LocalAuthority.Id.Should().Be(PostgresTestFixture.SeededData.LocalAuthority.Id);
        savedSchool.EstablishmentType.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentType.Id);
        savedSchool.EstablishmentGroup.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentGroup.Id);
        savedSchool.EstablishmentStatus.Id.Should().Be(PostgresTestFixture.SeededData.EstablishmentStatus.Id);
    }

    [Fact]
    public async Task ExistingSchoolByUrnButDifferentAlternateIdentifiers_Upsert_ReturnsUkprnError()
    {
        var URN = PostgresTestFixture.SeededData.NextSeedNumber();
        var UKPRN = PostgresTestFixture.SeededData.NextSeedNumber();
        var EstablishmentNumber = PostgresTestFixture.SeededData.NextSeedNumber();
        var seededSchool = await SeedSchool(URN, UKPRN, EstablishmentNumber, TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, ErrorResponse>(new()
        {
            URN = URN,
            UKPRN = 1234567890,
            EstablishmentNumber = 1234567890,
            Name = "Updated School",
            Address = new()
            {
                Street = "Street Updated",
                Locality = "Locality Updated",
                AddressThree = "Address Three Updated",
                Town = "Town Updated",
                County = "County Updated",
                PostCode = "Post Code Updated",
            },
            OpenDate = new DateOnly(2023, 10, 21),
            CloseDate = new DateOnly(2026, 10, 21),
            PhaseOfEducationId = PostgresTestFixture.SeededData.PhaseOfEducation.Id,
            LocalAuthorityId = PostgresTestFixture.SeededData.LocalAuthority.Id,
            EstablishmentTypeId = PostgresTestFixture.SeededData.EstablishmentType.Id,
            EstablishmentGroupId = PostgresTestFixture.SeededData.EstablishmentGroup.Id,
            EstablishmentStatusId = PostgresTestFixture.SeededData.EstablishmentStatus.Id,
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        res.Errors.Count.Should().Be(2);
        res.Errors.Keys.Should()
            .Equal("ukprn", "establishmentNumber");
        res.Errors.Values.SelectMany(v => v).Should()
            .Equal($"UKPRN does not match existing record with URN {URN}",
                   $"Establishment Number does not match existing record with URN {URN}");
    }

    [Fact]
    public async Task ExistingSchoolByUrn_Upsert_UpdatesSchoolSuccessfully()
    {
        var URN = PostgresTestFixture.SeededData.NextSeedNumber();
        var UKPRN = PostgresTestFixture.SeededData.NextSeedNumber();
        var EstablishmentNumber = PostgresTestFixture.SeededData.NextSeedNumber();
        var seededSchool = await SeedSchool(URN, UKPRN, EstablishmentNumber, TestContext.Current.CancellationToken);

        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededPhase = new PhaseOfEducation() { Code = PostgresTestFixture.SeededData.NextSeedNumber(), Name = "Phase" };
        seedingContext.Add(seededPhase);
        var seededAuthority = new LocalAuthority() { Code = PostgresTestFixture.SeededData.NextSeedNumber(), Name = "Authority" };
        seedingContext.Add(seededAuthority);
        var seededType = new EstablishmentType() { Code = PostgresTestFixture.SeededData.NextSeedNumber(), Name = "Type" };
        seedingContext.Add(seededType);
        var seededGroup = new EstablishmentGroup() { Code = PostgresTestFixture.SeededData.NextSeedNumber(), Name = "Group" };
        seedingContext.Add(seededGroup);
        var seededStatus = new EstablishmentStatus() { Code = PostgresTestFixture.SeededData.NextSeedNumber(), Name = "Status" };
        seedingContext.Add(seededStatus);
        await seedingContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (rsp, res) = await App.Client.POSTAsync<Create.Endpoint, Create.Request, Create.Response>(new()
        {
            URN = URN,
            UKPRN = UKPRN,
            EstablishmentNumber = EstablishmentNumber,
            Name = "Updated School",
            Address = new()
            {
                Street = "Street One Updated",
                Locality = "Locality Updated",
                AddressThree = "Address Three Updated",
                Town = "Town Updated",
                County = "County Updated",
                PostCode = "Post Code Updated",
            },
            OpenDate = new DateOnly(2023, 10, 21),
            CloseDate = new DateOnly(2026, 10, 21),
            PhaseOfEducationId = seededPhase.Id,
            LocalAuthorityId = seededAuthority.Id,
            EstablishmentTypeId = seededType.Id,
            EstablishmentGroupId = seededGroup.Id,
            EstablishmentStatusId = seededStatus.Id,
        });
        rsp.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Id.Should().NotBeEmpty();
        res.Id.Should().Be(seededSchool.Id);

        using var scope = App.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        var saveSchool = await dbContext.School.AsNoTracking().SingleAsync(s => s.Id == seededSchool.Id, cancellationToken: TestContext.Current.CancellationToken);
        saveSchool.Should().NotBeNull();
        saveSchool.Id.Should().Be(seededSchool.Id);
        saveSchool.URN.Should().Be(URN);
        saveSchool.UKPRN.Should().Be(UKPRN);
        saveSchool.EstablishmentNumber.Should().Be(EstablishmentNumber);
        saveSchool.Name.Should().Be("Updated School");
        saveSchool.Address.Should().NotBeNull();
        saveSchool.Address.Street.Should().Be("Street One Updated");
        saveSchool.Address.Locality.Should().Be("Locality Updated");
        saveSchool.Address.AddressThree.Should().Be("Address Three Updated");
        saveSchool.Address.Town.Should().Be("Town Updated");
        saveSchool.Address.County.Should().Be("County Updated");
        saveSchool.Address.PostCode.Should().Be("Post Code Updated");
        saveSchool.OpenDate.Should().Be(new DateOnly(2023, 10, 21));
        saveSchool.CloseDate.Should().Be(new DateOnly(2026, 10, 21));
        saveSchool.PhaseOfEducation.Id.Should().Be(seededPhase.Id);
        saveSchool.LocalAuthority.Id.Should().Be(seededAuthority.Id);
        saveSchool.EstablishmentType.Id.Should().Be(seededType.Id);
        saveSchool.EstablishmentGroup.Id.Should().Be(seededGroup.Id);
        saveSchool.EstablishmentStatus.Id.Should().Be(seededStatus.Id);
    }

    [Fact]
    public async Task InvalidRequest_Upsert_ReturnsValidationErrors()
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
    public async Task LinkedEntitiesDontExist_Upsert_ReturnsEntityErrors()
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


    private async Task<School> SeedSchool(int URN, int UKPRN, int EstablishmentNumber, CancellationToken c)
    {
        using var seedingScope = App.Services.CreateScope();
        await using var seedingContext = seedingScope.ServiceProvider.GetRequiredService<SchoolContext>();
        var seededSchool = new School
        {
            URN = URN,
            UKPRN = UKPRN,
            EstablishmentNumber = EstablishmentNumber,
            Name = "Existing School",
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
            PhaseOfEducation = (await seedingContext.PhaseOfEducation.FindAsync([PostgresTestFixture.SeededData.PhaseOfEducation.Id], c))!,
            LocalAuthority = (await seedingContext.LocalAuthority.FindAsync([PostgresTestFixture.SeededData.LocalAuthority.Id], c))!,
            EstablishmentType = (await seedingContext.EstablishmentType.FindAsync([PostgresTestFixture.SeededData.EstablishmentType.Id], c))!,
            EstablishmentGroup = (await seedingContext.EstablishmentGroup.FindAsync([PostgresTestFixture.SeededData.EstablishmentGroup.Id], c))!,
            EstablishmentStatus = (await seedingContext.EstablishmentStatus.FindAsync([PostgresTestFixture.SeededData.EstablishmentStatus.Id], c))!,
        };
        seedingContext.Add(seededSchool);
        await seedingContext.SaveChangesAsync(c);
        return seededSchool;
    }
}
