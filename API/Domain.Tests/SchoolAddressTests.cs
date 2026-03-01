using Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class SchoolAddressTests
{
    [Fact]
    public void SchoolAddress_ShouldHaveRequiredProperties()
    {
        // Arrange
        var address = new SchoolAddress
        {
            Town = "Test Town",
            PostCode = "TE1 2ST"
        };

        // Act & Assert
        Assert.NotNull(address.Town);
        Assert.NotNull(address.PostCode);
        Assert.Equal("Test Town", address.Town);
        Assert.Equal("TE1 2ST", address.PostCode);
    }

    [Fact]
    public void SchoolAddress_OptionalProperties_CanBeNull()
    {
        // Arrange
        var address = new SchoolAddress
        {
            Town = "Test Town",
            PostCode = "TE1 2ST"
        };

        // Act & Assert
        Assert.Null(address.Street);
        Assert.Null(address.Locality);
        Assert.Null(address.AddressThree);
        Assert.Null(address.County);
    }

    [Fact]
    public void SchoolAddress_Version_ShouldBeZeroByDefault()
    {
        // Arrange
        var address = new SchoolAddress
        {
            Town = "Test Town",
            PostCode = "TE1 2ST"
        };

        // Act & Assert
        Assert.Equal(0u, address.Version);
    }

    [Fact]
    public void SchoolAddress_AllProperties_CanBeSet()
    {
        // Arrange
        var address = new SchoolAddress
        {
            Street = "123 Test Street",
            Locality = "Test Area",
            AddressThree = "Test Building",
            Town = "Test Town",
            County = "Test County",
            PostCode = "TE1 2ST",
            Version = 1
        };

        // Act & Assert
        Assert.Equal("123 Test Street", address.Street);
        Assert.Equal("Test Area", address.Locality);
        Assert.Equal("Test Building", address.AddressThree);
        Assert.Equal("Test Town", address.Town);
        Assert.Equal("Test County", address.County);
        Assert.Equal("TE1 2ST", address.PostCode);
        Assert.Equal(1u, address.Version);
    }
}