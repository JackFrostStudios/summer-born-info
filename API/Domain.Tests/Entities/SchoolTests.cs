namespace SummerBornInfo.Domain.Tests.Entities;

public class SchoolTests
{
    [Fact]
    public void School_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var school = new School
        {
            Id = Guid.NewGuid(),
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = new SchoolAddress
            {
                Town = "Test Town",
                PostCode = "TE1 2ST"
            },
            PhaseOfEducationId = Guid.NewGuid(),
            PhaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" },
            LocalAuthorityId = Guid.NewGuid(),
            LocalAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" },
            EstablishmentTypeId = Guid.NewGuid(),
            EstablishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" },
            EstablishmentGroupId = Guid.NewGuid(),
            EstablishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" },
            EstablishmentStatusId = Guid.NewGuid(),
            EstablishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" }
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(school.Name));
    }

    [Fact]
    public void School_URN_ShouldNotBeZero()
    {
        // Arrange
        var school = new School
        {
            Id = Guid.NewGuid(),
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = new SchoolAddress
            {
                Town = "Test Town",
                PostCode = "TE1 2ST"
            },
            PhaseOfEducationId = Guid.NewGuid(),
            PhaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" },
            LocalAuthorityId = Guid.NewGuid(),
            LocalAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" },
            EstablishmentTypeId = Guid.NewGuid(),
            EstablishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" },
            EstablishmentGroupId = Guid.NewGuid(),
            EstablishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" },
            EstablishmentStatusId = Guid.NewGuid(),
            EstablishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" }
        };

        // Act & Assert
        Assert.NotEqual(0, school.URN);
    }

    [Fact]
    public void School_EstablishmentNumber_ShouldNotBeZero()
    {
        // Arrange
        var school = new School
        {
            Id = Guid.NewGuid(),
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = new SchoolAddress
            {
                Town = "Test Town",
                PostCode = "TE1 2ST"
            },
            PhaseOfEducationId = Guid.NewGuid(),
            PhaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" },
            LocalAuthorityId = Guid.NewGuid(),
            LocalAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" },
            EstablishmentTypeId = Guid.NewGuid(),
            EstablishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" },
            EstablishmentGroupId = Guid.NewGuid(),
            EstablishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" },
            EstablishmentStatusId = Guid.NewGuid(),
            EstablishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" }
        };

        // Act & Assert
        Assert.NotEqual(0, school.EstablishmentNumber);
    }

    [Fact]
    public void School_ShouldHaveValidAddress()
    {
        // Arrange
        var address = new SchoolAddress
        {
            Town = "Test Town",
            PostCode = "TE1 2ST"
        };
        
        var school = new School
        {
            Id = Guid.NewGuid(),
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = address,
            PhaseOfEducationId = Guid.NewGuid(),
            PhaseOfEducation = new PhaseOfEducation { Id = Guid.NewGuid(), Code = "PRI", Name = "Primary" },
            LocalAuthorityId = Guid.NewGuid(),
            LocalAuthority = new LocalAuthority { Id = Guid.NewGuid(), Code = "LA01", Name = "Test LA" },
            EstablishmentTypeId = Guid.NewGuid(),
            EstablishmentType = new EstablishmentType { Id = Guid.NewGuid(), Code = "AC", Name = "Academy" },
            EstablishmentGroupId = Guid.NewGuid(),
            EstablishmentGroup = new EstablishmentGroup { Id = Guid.NewGuid(), Code = "SCH", Name = "School" },
            EstablishmentStatusId = Guid.NewGuid(),
            EstablishmentStatus = new EstablishmentStatus { Id = Guid.NewGuid(), Code = "OPEN", Name = "Open" }
        };

        // Act & Assert
        Assert.NotNull(school.Address);
        Assert.Equal("Test Town", school.Address.Town);
        Assert.Equal("TE1 2ST", school.Address.PostCode);
    }

    [Fact]
    public void School_ShouldHaveRequiredRelationships()
    {
        // Arrange
        var phaseOfEducationId = Guid.NewGuid();
        var localAuthorityId = Guid.NewGuid();
        var establishmentTypeId = Guid.NewGuid();
        var establishmentGroupId = Guid.NewGuid();
        var establishmentStatusId = Guid.NewGuid();
        
        var school = new School
        {
            Id = Guid.NewGuid(),
            URN = 123456,
            EstablishmentNumber = 1234,
            Name = "Test School",
            Address = new SchoolAddress
            {
                Town = "Test Town",
                PostCode = "TE1 2ST"
            },
            PhaseOfEducationId = phaseOfEducationId,
            PhaseOfEducation = new PhaseOfEducation { Id = phaseOfEducationId, Code = "PRI", Name = "Primary" },
            LocalAuthorityId = localAuthorityId,
            LocalAuthority = new LocalAuthority { Id = localAuthorityId, Code = "LA01", Name = "Test LA" },
            EstablishmentTypeId = establishmentTypeId,
            EstablishmentType = new EstablishmentType { Id = establishmentTypeId, Code = "AC", Name = "Academy" },
            EstablishmentGroupId = establishmentGroupId,
            EstablishmentGroup = new EstablishmentGroup { Id = establishmentGroupId, Code = "SCH", Name = "School" },
            EstablishmentStatusId = establishmentStatusId,
            EstablishmentStatus = new EstablishmentStatus { Id = establishmentStatusId, Code = "OPEN", Name = "Open" }
        };

        // Act & Assert
        Assert.NotNull(school.PhaseOfEducation);
        Assert.NotNull(school.LocalAuthority);
        Assert.NotNull(school.EstablishmentType);
        Assert.NotNull(school.EstablishmentGroup);
        Assert.NotNull(school.EstablishmentStatus);
        
        Assert.Equal(phaseOfEducationId, school.PhaseOfEducationId);
        Assert.Equal(localAuthorityId, school.LocalAuthorityId);
        Assert.Equal(establishmentTypeId, school.EstablishmentTypeId);
        Assert.Equal(establishmentGroupId, school.EstablishmentGroupId);
        Assert.Equal(establishmentStatusId, school.EstablishmentStatusId);
    }
}
