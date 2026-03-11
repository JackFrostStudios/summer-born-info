namespace SummerBornInfo.Domain.Tests.Entities;

public class LocalAuthorityTests
{
    [Fact]
    public void LocalAuthority_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        var localAuthority = new LocalAuthority
        {
            Code = "E09000001",
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.Equal("E09000001", localAuthority.Code);
        Assert.Equal("Local Authority One", localAuthority.Name);
    }

    [Fact]
    public void LocalAuthority_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var localAuthority = new LocalAuthority
        {
            Id = id,
            Code = "E09000001",
            Name = "Local Authority One",
            Version = 10u
        };

        // Act & Assert
        Assert.Equal(id, localAuthority.Id);
        Assert.Equal("E09000001", localAuthority.Code);
        Assert.Equal("Local Authority One", localAuthority.Name);
        Assert.Equal(10u, localAuthority.Version);
    }
}