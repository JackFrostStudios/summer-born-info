namespace SummerBornInfo.Domain.Tests.Entities;

public class PhaseOfEducationTests
{
    [Fact]
    public void PhaseOfEducation_ShouldHaveRequiredProperties()
    {
        // Arrange
        var phase = new PhaseOfEducation
        {
            Id = Guid.NewGuid(),
            Code = "PRI",
            Name = "Primary"
        };

        // Act & Assert
        Assert.Equal(Guid.NewGuid().GetType(), phase.Id.GetType());
        Assert.Equal("PRI", phase.Code);
        Assert.Equal("Primary", phase.Name);
    }

    [Fact]
    public void PhaseOfEducation_Version_ShouldBeZeroByDefault()
    {
        // Arrange
        var phase = new PhaseOfEducation
        {
            Id = Guid.NewGuid(),
            Code = "PRI",
            Name = "Primary"
        };

        // Act & Assert
        Assert.Equal(0u, phase.Version);
    }

    [Fact]
    public void PhaseOfEducation_AllProperties_CanBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var phase = new PhaseOfEducation
        {
            Id = id,
            Code = "SEC",
            Name = "Secondary",
            Version = 1
        };

        // Act & Assert
        Assert.Equal(id, phase.Id);
        Assert.Equal("SEC", phase.Code);
        Assert.Equal("Secondary", phase.Name);
        Assert.Equal(1u, phase.Version);
    }

    [Fact]
    public void PhaseOfEducation_Code_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var phase = new PhaseOfEducation
        {
            Id = Guid.NewGuid(),
            Code = "PRI",
            Name = "Primary"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(phase.Code));
    }

    [Fact]
    public void PhaseOfEducation_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var phase = new PhaseOfEducation
        {
            Id = Guid.NewGuid(),
            Code = "PRI",
            Name = "Primary"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(phase.Name));
    }
}