namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class EstablishmentTypeTests
{
    [Fact]
    public void EstablishmentType_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        EstablishmentType establishmentType = new()
        {
            Code = "AC",
            Name = "Academy",
        };

        // Act & Assert
        Assert.Equal("AC", establishmentType.Code);
        Assert.Equal("Academy", establishmentType.Name);
    }

    [Fact]
    public void EstablishmentType_ShouldGenerateId()
    {
        // Arrange
        EstablishmentType establishmentType = new()
        {
            Code = "AC",
            Name = "Academy",
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, establishmentType.Id);
    }

    [Fact]
    public void EstablishmentType_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        EstablishmentType establishmentType = new()
        {
            Id = id,
            Code = "AC",
            Name = "Academy",
        };

        // Act & Assert
        Assert.Equal(id, establishmentType.Id);
        Assert.Equal("AC", establishmentType.Code);
        Assert.Equal("Academy", establishmentType.Name);
    }
}
