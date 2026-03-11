namespace SummerBornInfo.Domain.Tests.Entities;

public class EstablishmentTypeTests
{
    [Fact]
    public void EstablishmentType_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        var establishmentType = new EstablishmentType
        {
            Code = "AC",
            Name = "Academy"
        };

        // Act & Assert
        Assert.Equal("AC", establishmentType.Code);
        Assert.Equal("Academy", establishmentType.Name);
    }

    [Fact]
    public void EstablishmentType_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var establishmentType = new EstablishmentType
        {
            Id = id,
            Code = "AC",
            Name = "Academy",
            Version = 10u
        };

        // Act & Assert
        Assert.Equal(id, establishmentType.Id);
        Assert.Equal("AC", establishmentType.Code);
        Assert.Equal("Academy", establishmentType.Name);
        Assert.Equal(10u, establishmentType.Version);
    }
}