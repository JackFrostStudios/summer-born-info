namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class SchoolAddressTests
{
    [Fact]
    public void SchoolAddress_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        SchoolAddress address = new()
        {
            Town = "Test Town",
            PostCode = "TE1 2ST",
        };

        // Act & Assert
        Assert.Equal("Test Town", address.Town);
        Assert.Equal("TE1 2ST", address.PostCode);
    }

    [Fact]
    public void SchoolAddress_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        SchoolAddress address = new()
        {
            Street = "123 Test Street",
            Locality = "Test Area",
            AddressThree = "Test Building",
            Town = "Test Town",
            County = "Test County",
            PostCode = "TE1 2ST",
        };

        // Act & Assert
        Assert.Equal("123 Test Street", address.Street);
        Assert.Equal("Test Area", address.Locality);
        Assert.Equal("Test Building", address.AddressThree);
        Assert.Equal("Test Town", address.Town);
        Assert.Equal("Test County", address.County);
        Assert.Equal("TE1 2ST", address.PostCode);
    }
}