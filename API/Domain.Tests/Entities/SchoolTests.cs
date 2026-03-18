namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class SchoolTests
{
    [Fact]
    public void School_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        var address = new SchoolAddress { Town = "Test Town", PostCode = "TE1 2ST" };
        var phaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        var localAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        var establishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        var establishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        var establishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };

        var school = new School
        {
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus
        };

        // Act & Assert 
        Assert.Equal(123456, school.URN);
        Assert.Equal("Test School", school.Name);
        Assert.Equal(address, school.Address);

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
        var address = new SchoolAddress { Town = "Test Town", PostCode = "TE1 2ST" };
        var phaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        var localAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        var establishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        var establishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        var establishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };

        var school = new School
        {
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            PhaseOfEducation = phaseOfEducation,
            LocalAuthority = localAuthority,
            EstablishmentType = establishmentType,
            EstablishmentGroup = establishmentGroup,
            EstablishmentStatus = establishmentStatus
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, school.Id);
    }

    [Fact]
    public void School_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var schoolId = Guid.NewGuid();
        var address = new SchoolAddress { Town = "Test Town", PostCode = "TE1 2ST" };
        var openDate = new DateOnly(2026, 1, 1);
        var closeDate = new DateOnly(2026, 2, 1);
        var phaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" };
        var localAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" };
        var establishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" };
        var establishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" };
        var establishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" };
        
        var school = new School
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
            Version = 10u
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

        Assert.Equal(10u, school.Version);
    }
}
