namespace SummerBornInfo.Domain.Tests.Entities;

public class EstablishmentGroupTests
{
    [Fact]
    public void EstablishmentGroup_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        var establishmentGroup = new EstablishmentGroup
        {
            Code = "SCH",
            Name = "School"
        };

        // Act & Assert
        Assert.Equal("SCH", establishmentGroup.Code);
        Assert.Equal("School", establishmentGroup.Name);
    }

    [Fact]
    public void EstablishmentGroup_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var establishmentGroup = new EstablishmentGroup
        {
            Id = id,
            Code = "SCH",
            Name = "School",
            Version = 10u
        };

        // Act & Assert
        Assert.Equal(id, establishmentGroup.Id);
        Assert.Equal("SCH", establishmentGroup.Code);
        Assert.Equal("School", establishmentGroup.Name);
        Assert.Equal(10u, establishmentGroup.Version);
    }
}