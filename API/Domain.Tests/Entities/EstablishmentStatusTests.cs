namespace SummerBornInfo.Domain.Tests.Entities;

public class EstablishmentStatusTests
{
    [Fact]
    public void EstablishmentStatus_ShouldHaveRequiredProperties()
    {
        // Arrange
        var establishmentStatus = new EstablishmentStatus
        {
            Id = Guid.NewGuid(),
            Code = "OPEN",
            Name = "Open"
        };

        // Act & Assert
        Assert.Equal(Guid.NewGuid().GetType(), establishmentStatus.Id.GetType());
        Assert.Equal("OPEN", establishmentStatus.Code);
        Assert.Equal("Open", establishmentStatus.Name);
    }

    [Fact]
    public void EstablishmentStatus_Version_ShouldBeZeroByDefault()
    {
        // Arrange
        var establishmentStatus = new EstablishmentStatus
        {
            Id = Guid.NewGuid(),
            Code = "OPEN",
            Name = "Open"
        };

        // Act & Assert
        Assert.Equal(0u, establishmentStatus.Version);
    }

    [Fact]
    public void EstablishmentStatus_AllProperties_CanBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var establishmentStatus = new EstablishmentStatus
        {
            Id = id,
            Code = "CLOSED",
            Name = "Closed",
            Version = 1
        };

        // Act & Assert
        Assert.Equal(id, establishmentStatus.Id);
        Assert.Equal("CLOSED", establishmentStatus.Code);
        Assert.Equal("Closed", establishmentStatus.Name);
        Assert.Equal(1u, establishmentStatus.Version);
    }

    [Fact]
    public void EstablishmentStatus_Code_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var establishmentStatus = new EstablishmentStatus
        {
            Id = Guid.NewGuid(),
            Code = "OPEN",
            Name = "Open"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(establishmentStatus.Code));
    }

    [Fact]
    public void EstablishmentStatus_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var establishmentStatus = new EstablishmentStatus
        {
            Id = Guid.NewGuid(),
            Code = "OPEN",
            Name = "Open"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(establishmentStatus.Name));
    }
}