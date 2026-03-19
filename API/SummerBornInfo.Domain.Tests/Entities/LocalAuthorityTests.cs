namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class LocalAuthorityTests
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
    public void LocalAuthority_ShouldGenerateId()
    {
        // Arrange
        var localAuthority = new LocalAuthority
        {
            Code = "E09000001",
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, localAuthority.Id);
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
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.Equal(id, localAuthority.Id);
        Assert.Equal("E09000001", localAuthority.Code);
        Assert.Equal("Local Authority One", localAuthority.Name);
    }
}