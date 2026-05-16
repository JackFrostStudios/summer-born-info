namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class EstablishmentGroupTests
{
    [Fact]
    public void EstablishmentGroup_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        EstablishmentGroup establishmentGroup = new()
        {
            Code = "SCH",
            Name = "School",
        };

        // Act & Assert
        Assert.Equal("SCH", establishmentGroup.Code);
        Assert.Equal("School", establishmentGroup.Name);
    }

    [Fact]
    public void EstablishmentGroup_ShouldGenerateId()
    {
        // Arrange
        EstablishmentGroup establishmentGroup = new()
        {
            Code = "SCH",
            Name = "School",
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, establishmentGroup.Id);
    }

    [Fact]
    public void EstablishmentGroup_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        EstablishmentGroup establishmentGroup = new()
        {
            Id = id,
            Code = "SCH",
            Name = "School",
        };

        // Act & Assert
        Assert.Equal(id, establishmentGroup.Id);
        Assert.Equal("SCH", establishmentGroup.Code);
        Assert.Equal("School", establishmentGroup.Name);
    }
}