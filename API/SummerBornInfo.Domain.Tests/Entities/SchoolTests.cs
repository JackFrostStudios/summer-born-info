namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class SchoolTests
{
    [Fact]
    public void School_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        SchoolAddress address = new() { Town = "Test Town", PostCode = "TE1 2ST" };
        PhaseOfEducation phaseOfEducation = new() { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        LocalAuthority localAuthority = new() { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        EstablishmentType establishmentType = new() { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        EstablishmentGroup establishmentGroup = new() { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        EstablishmentStatus establishmentStatus = new() { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };

        School school = new()
        {
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
        };

        // Act & Assert 
        Assert.Equal(123456, school.URN);
        Assert.Equal("Test School", school.Name);
        Assert.Equal(address, school.Address);
        Assert.Null(school.Location);

        Assert.Equal(phaseOfEducation, school.PhaseOfEducation);
        Assert.Equal(localAuthority, school.LocalAuthority);
        Assert.Equal(establishmentType, school.EstablishmentType);
        Assert.Equal(establishmentGroup, school.EstablishmentGroup);
        Assert.Equal(establishmentStatus, school.EstablishmentStatus);
    }

    [Fact]
    public void School_ShouldGenerateId()
    {
        // Arrange
        SchoolAddress address = new() { Town = "Test Town", PostCode = "TE1 2ST" };
        PhaseOfEducation phaseOfEducation = new() { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        LocalAuthority localAuthority = new() { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        EstablishmentType establishmentType = new() { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        EstablishmentGroup establishmentGroup = new() { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        EstablishmentStatus establishmentStatus = new() { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };

        School school = new()
        {
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, school.Id);
    }

    [Fact]
    public void School_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var schoolId = Guid.NewGuid();
        SchoolAddress address = new() { Town = "Test Town", PostCode = "TE1 2ST" };
        DateOnly openDate = new(2026, 1, 1);
        DateOnly closeDate = new(2026, 2, 1);
        PhaseOfEducation phaseOfEducation = new() { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        LocalAuthority localAuthority = new() { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        EstablishmentType establishmentType = new() { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        EstablishmentGroup establishmentGroup = new() { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        EstablishmentStatus establishmentStatus = new() { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };

        School school = new()
        {
            Id = schoolId,
            URN = 123456,
            UKPRN = 789,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            OpenDate = openDate,
            CloseDate = closeDate,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
        };

        // Act & Assert 
        Assert.Equal(schoolId, school.Id);
        Assert.Equal(123456, school.URN);
        Assert.Equal(789, school.UKPRN);
        Assert.Equal("Test School", school.Name);
        Assert.Equal(address, school.Address);
        Assert.Equal(openDate, school.OpenDate);
        Assert.Equal(closeDate, school.CloseDate);

        Assert.Equal(phaseOfEducation, school.PhaseOfEducation);
        Assert.Equal(localAuthority, school.LocalAuthority);
        Assert.Equal(establishmentType, school.EstablishmentType);
        Assert.Equal(establishmentGroup, school.EstablishmentGroup);
        Assert.Equal(establishmentStatus, school.EstablishmentStatus);
    }

    [Fact]
    public void School_ShouldAllowCanonicalLocationAssignment()
    {
        // Arrange
        SchoolAddress address = new() { Town = "Test Town", PostCode = "TE1 2ST" };
        PhaseOfEducation phaseOfEducation = new() { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        LocalAuthority localAuthority = new() { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        EstablishmentType establishmentType = new() { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        EstablishmentGroup establishmentGroup = new() { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        EstablishmentStatus establishmentStatus = new() { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };
        var location = new NetTopologySuite.Geometries.Point(-1.5491, 53.8008) { SRID = 4326 };

        // Act
        School school = new()
        {
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            Location = location,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus,
        };

        // Assert
        Assert.Same(location, school.Location);
        Assert.Equal(4326, school.Location.SRID);
        Assert.Equal(-1.5491, school.Location.X);
        Assert.Equal(53.8008, school.Location.Y);
    }
}
