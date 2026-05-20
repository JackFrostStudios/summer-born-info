namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class EstablishmentStatusTests
{
    [Fact]
    public void EstablishmentStatus_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        EstablishmentStatus establishmentStatus = new()
        {
            Code = "OPEN",
            Name = "Open",
        };

        // Act & Assert
        Assert.Equal("OPEN", establishmentStatus.Code);
        Assert.Equal("Open", establishmentStatus.Name);
    }

    [Fact]
    public void EstablishmentStatus_ShouldGenerateId()
    {
        // Arrange
        EstablishmentStatus establishmentStatus = new()
        {
            Code = "OPEN",
            Name = "Open",
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, establishmentStatus.Id);
    }

    [Fact]
    public void EstablishmentStatus_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        EstablishmentStatus establishmentStatus = new()
        {
            Id = id,
            Code = "OPEN",
            Name = "Open",
        };

        // Act & Assert
        Assert.Equal(id, establishmentStatus.Id);
        Assert.Equal("OPEN", establishmentStatus.Code);
        Assert.Equal("Open", establishmentStatus.Name);
    }
}
