namespace SummerBornInfo.Infrastructure.Tests.Persistence;

public sealed class ApplicationDbContextSchoolTests(IntegrationTestDatabaseServerFixture testDatabaseServerFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public void GivenSchoolPersistenceModel_WhenGeneratingCreateScript_ThenSearchAndSpatialFoundationIsIncluded()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        // Act
        var createScript = dbContext.Database.GenerateCreateScript();

        // Assert
        Assert.Contains("CREATE EXTENSION IF NOT EXISTS pg_trgm;", createScript, StringComparison.Ordinal);
        Assert.Contains("CREATE EXTENSION IF NOT EXISTS postgis;", createScript, StringComparison.Ordinal);
        Assert.Contains(@"""SchoolGeometry"" geography (point, 4326)", createScript, StringComparison.Ordinal);
        Assert.Contains("search_vector", createScript, StringComparison.Ordinal);
        Assert.Contains("GENERATED ALWAYS AS", createScript, StringComparison.Ordinal);
        Assert.Contains(@"to_tsvector('simple', coalesce(""Name"", ''))", createScript, StringComparison.Ordinal);
        Assert.Contains(@"lower(coalesce(""Name"", ''))", createScript, StringComparison.Ordinal);
        Assert.Contains(@"replace(lower(coalesce(""PostCode"", '')), ' ', '')", createScript, StringComparison.Ordinal);
        Assert.Contains("search_address_normalized", createScript, StringComparison.Ordinal);
        Assert.Contains("ix_school_urn", createScript, StringComparison.Ordinal);
        Assert.Contains("UNIQUE", createScript, StringComparison.Ordinal);
        Assert.Contains("ix_school_school_geometry", createScript, StringComparison.Ordinal);
        Assert.Contains("ix_school_search_vector", createScript, StringComparison.Ordinal);
        Assert.Contains("ix_school_search_name_normalized", createScript, StringComparison.Ordinal);
        Assert.Contains("ix_school_search_postcode_normalized", createScript, StringComparison.Ordinal);
        Assert.Contains("ix_school_search_address_normalized", createScript, StringComparison.Ordinal);
        Assert.Contains("USING gist", createScript, StringComparison.Ordinal);
        Assert.Contains("USING gin", createScript, StringComparison.Ordinal);
        Assert.Contains("gin_trgm_ops", createScript, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GivenPersistedSchool_WhenReadingGeneratedSearchColumns_ThenNormalizedValuesAreStored()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        school.Name = "St Example Academy";
        school.Address.Street = "10 Market Road";
        school.Address.Locality = "Old Quarter";
        school.Address.AddressThree = null;
        school.Address.Town = "Leeds";
        school.Address.County = "West Yorkshire";
        school.Address.PostCode = "LS1 2AB";

        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var command = dbContext.GetNpgsqlConnection().CreateCommand();
        command.CommandText =
            "SELECT " +
            "search_name_normalized, " +
            "search_postcode_normalized, " +
            "search_address_normalized, " +
            "search_vector::text " +
            "FROM school " +
            "WHERE \"Id\" = @id";
        _ = command.Parameters.AddWithValue("id", school.Id);

        if (command.Connection?.State != System.Data.ConnectionState.Open)
        {
            await command.Connection!.OpenAsync(TestContext.Current.CancellationToken);
        }

        // Act
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken));

        var searchNameNormalized = reader.GetString(0);
        var searchPostcodeNormalized = reader.GetString(1);
        var searchAddressNormalized = reader.GetString(2);
        var searchVector = reader.GetString(3);

        // Assert
        Assert.Equal("st example academy", searchNameNormalized);
        Assert.Equal("ls12ab", searchPostcodeNormalized);
        Assert.Equal("10 market road old quarter  leeds west yorkshire ls1 2ab", searchAddressNormalized);
        Assert.Contains("'st'", searchVector, StringComparison.Ordinal);
        Assert.Contains("'example'", searchVector, StringComparison.Ordinal);
        Assert.Contains("'academy'", searchVector, StringComparison.Ordinal);
        Assert.Contains("'ls1'", searchVector, StringComparison.Ordinal);
        Assert.Contains("'2ab'", searchVector, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GivenSchoolHasLocation_WhenPersisted_ThenSpatialPointRoundTripsThroughPostGisMapping()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        school.Geometry = new NetTopologySuite.Geometries.Point(-1.5491d, 53.8008d) { SRID = 4326 };

        // Act
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.NotNull(savedSchool.Geometry);
        Assert.Equal(4326, savedSchool.Geometry.SRID);
        Assert.Equal(school.Geometry.X, savedSchool.Geometry.X, precision: 6);
        Assert.Equal(school.Geometry.Y, savedSchool.Geometry.Y, precision: 6);
    }

    [Fact]
    public async Task GivenNewSchool_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();

        // Act
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(school, savedSchool);
    }

    [Fact]
    public async Task GivenNewSchoolWithOnlyRequiredFields_WhenInsertingToDatabase_ThenRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        school.UKPRN = null;
        school.OpenDate = null;
        school.CloseDate = null;
        school.Address.Street = null;
        school.Address.Locality = null;
        school.Address.AddressThree = null;
        school.Address.County = null;

        // Act
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(school, savedSchool);
    }

    [Fact]
    public async Task GivenExistingSchoolUrn_WhenInsertingSecondSchoolWithSameUrn_ThenSaveFails()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var firstSchool = SchoolFactory.GetSchool();
        var secondSchool = SchoolFactory.GetSchool();
        secondSchool.URN = firstSchool.URN;

        _ = dbContext.Schools.Add(firstSchool);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _ = dbContext.Schools.Add(secondSchool);

        // Act
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        // Assert
        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.Equal("ix_school_urn", postgresException.ConstraintName);
    }

    [Fact]
    public async Task GivenExistingSchool_WhenUpdatingAllFields_ThenUpdatedRecordCanBeRetrieved()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var schoolToUpdate = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(schoolToUpdate);
        schoolToUpdate.URN = 999999999;
        schoolToUpdate.UKPRN = 999999999;
        schoolToUpdate.EstablishmentNumber = 999999999;
        schoolToUpdate.Address = SchoolAddressFactory.GetSchoolAddress();
        schoolToUpdate.OpenDate = school.OpenDate?.AddDays(10);
        schoolToUpdate.CloseDate = school.CloseDate?.AddDays(10);
        schoolToUpdate.PhaseOfEducation = PhaseOfEducationFactory.GetPhaseOfEducation();
        schoolToUpdate.LocalAuthority = LocalAuthorityFactory.GetLocalAuthority();
        schoolToUpdate.EstablishmentType = EstablishmentTypeFactory.GetEstablishmentType();
        schoolToUpdate.EstablishmentGroup = EstablishmentGroupFactory.GetEstablishmentGroup();
        schoolToUpdate.EstablishmentStatus = EstablishmentStatusFactory.GetEstablishmentStatus();

        // Act
        _ = dbContext.PhasesOfEducation.Add(schoolToUpdate.PhaseOfEducation);
        _ = dbContext.LocalAuthorities.Add(schoolToUpdate.LocalAuthority);
        _ = dbContext.EstablishmentTypes.Add(schoolToUpdate.EstablishmentType);
        _ = dbContext.EstablishmentGroups.Add(schoolToUpdate.EstablishmentGroup);
        _ = dbContext.PhasesOfEducation.Add(schoolToUpdate.PhaseOfEducation);
        _ = dbContext.EstablishmentStatuses.Add(schoolToUpdate.EstablishmentStatus);
        _ = dbContext.Schools.Update(schoolToUpdate);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Assert
        dbContext.ChangeTracker.Clear();
        var savedSchool = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedSchool);
        Assert.Equivalent(schoolToUpdate, savedSchool);
    }

    [Fact]
    public async Task GivenExistingSchool_ConcurrentUpdates_ThenSecondUpdateShouldFail()
    {
        // Arrange
        var dbContext = CreateDbContext();
        var school = SchoolFactory.GetSchool();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContext.ChangeTracker.Clear();

        var schoolToUpdateOne = await dbContext.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(schoolToUpdateOne);
        schoolToUpdateOne.URN = 999999999;

        var dbContextTwo = CreateDbContext();
        var schoolToUpdateTwo = await dbContextTwo.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.NotNull(schoolToUpdateTwo);
        schoolToUpdateTwo.URN = 999999998;
        _ = await dbContextTwo.SaveChangesAsync(TestContext.Current.CancellationToken);
        dbContextTwo.ChangeTracker.Clear();

        // Act & Assert
        _ = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        var savedSchool = await dbContextTwo.Schools.FindAsync([school.Id], TestContext.Current.CancellationToken);
        Assert.Equivalent(schoolToUpdateTwo, savedSchool);
    }
}
