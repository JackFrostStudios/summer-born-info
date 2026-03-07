namespace SummerBornInfo.Domain.Tests.Entities;

public class EstablishmentTypeTests
{
    [Fact]
    public void EstablishmentType_ShouldHaveRequiredProperties()
    {
        // Arrange
        var establishmentType = new EstablishmentType
        {
            Id = Guid.NewGuid(),
            Code = "AC",
            Name = "Academy"
        };

        // Act & Assert
        Assert.Equal(Guid.NewGuid().GetType(), establishmentType.Id.GetType());
        Assert.Equal("AC", establishmentType.Code);
        Assert.Equal("Academy", establishmentType.Name);
    }

    [Fact]
    public void EstablishmentType_Version_ShouldBeZeroByDefault()
    {
        // Arrange
        var establishmentType = new EstablishmentType
        {
            Id = Guid.NewGuid(),
            Code = "AC",
            Name = "Academy"
        };

        // Act & Assert
        Assert.Equal(0u, establishmentType.Version);
    }

    [Fact]
    public void EstablishmentType_AllProperties_CanBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var establishmentType = new EstablishmentType
        {
            Id = id,
            Code = "LA",
            Name = "Local Authority",
            Version = 1
        };

        // Act & Assert
        Assert.Equal(id, establishmentType.Id);
        Assert.Equal("LA", establishmentType.Code);
        Assert.Equal("Local Authority", establishmentType.Name);
        Assert.Equal(1u, establishmentType.Version);
    }

    [Fact]
    public void EstablishmentType_Code_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var establishmentType = new EstablishmentType
        {
            Id = Guid.NewGuid(),
            Code = "AC",
            Name = "Academy"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(establishmentType.Code));
    }

    [Fact]
    public void EstablishmentType_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var establishmentType = new EstablishmentType
        {
            Id = Guid.NewGuid(),
            Code = "AC",
            Name = "Academy"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(establishmentType.Name));
    }
}