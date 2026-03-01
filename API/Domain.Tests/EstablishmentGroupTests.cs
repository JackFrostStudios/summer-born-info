using Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class EstablishmentGroupTests
{
    [Fact]
    public void EstablishmentGroup_ShouldHaveRequiredProperties()
    {
        // Arrange
        var establishmentGroup = new EstablishmentGroup
        {
            Id = Guid.NewGuid(),
            Code = "SCH",
            Name = "School"
        };

        // Act & Assert
        Assert.Equal(Guid.NewGuid().GetType(), establishmentGroup.Id.GetType());
        Assert.Equal("SCH", establishmentGroup.Code);
        Assert.Equal("School", establishmentGroup.Name);
    }

    [Fact]
    public void EstablishmentGroup_Version_ShouldBeZeroByDefault()
    {
        // Arrange
        var establishmentGroup = new EstablishmentGroup
        {
            Id = Guid.NewGuid(),
            Code = "SCH",
            Name = "School"
        };

        // Act & Assert
        Assert.Equal(0u, establishmentGroup.Version);
    }

    [Fact]
    public void EstablishmentGroup_AllProperties_CanBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var establishmentGroup = new EstablishmentGroup
        {
            Id = id,
            Code = "OTH",
            Name = "Other",
            Version = 1
        };

        // Act & Assert
        Assert.Equal(id, establishmentGroup.Id);
        Assert.Equal("OTH", establishmentGroup.Code);
        Assert.Equal("Other", establishmentGroup.Name);
        Assert.Equal(1u, establishmentGroup.Version);
    }

    [Fact]
    public void EstablishmentGroup_Code_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var establishmentGroup = new EstablishmentGroup
        {
            Id = Guid.NewGuid(),
            Code = "SCH",
            Name = "School"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(establishmentGroup.Code));
    }

    [Fact]
    public void EstablishmentGroup_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var establishmentGroup = new EstablishmentGroup
        {
            Id = Guid.NewGuid(),
            Code = "SCH",
            Name = "School"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(establishmentGroup.Name));
    }
}