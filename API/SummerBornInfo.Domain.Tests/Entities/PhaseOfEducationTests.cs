namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class PhaseOfEducationTests
{
    [Fact]
    public void PhaseOfEducation_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        PhaseOfEducation phase = new()
        {
            Code = "PRI",
            Name = "Primary",
        };

        // Act & Assert
        Assert.Equal("PRI", phase.Code);
        Assert.Equal("Primary", phase.Name);
    }

    [Fact]
    public void PhaseOfEducation_ShouldGenerateId()
    {
        // Arrange
        PhaseOfEducation phase = new()
        {
            Code = "PRI",
            Name = "Primary",
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, phase.Id);
    }

    [Fact]
    public void PhaseOfEducation_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        PhaseOfEducation phase = new()
        {
            Id = id,
            Code = "PRI",
            Name = "Primary",
        };

        // Act & Assert
        Assert.Equal(id, phase.Id);
        Assert.Equal("PRI", phase.Code);
        Assert.Equal("Primary", phase.Name);
    }
}
