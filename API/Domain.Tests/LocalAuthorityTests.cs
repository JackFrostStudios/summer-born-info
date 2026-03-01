using Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class LocalAuthorityTests
{
    [Fact]
    public void LocalAuthority_ShouldHaveRequiredProperties()
    {
        // Arrange
        var localAuthority = new LocalAuthority
        {
            Id = Guid.NewGuid(),
            Code = "E09000001",
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.Equal(Guid.NewGuid().GetType(), localAuthority.Id.GetType());
        Assert.Equal("E09000001", localAuthority.Code);
        Assert.Equal("Local Authority One", localAuthority.Name);
    }

    [Fact]
    public void LocalAuthority_Version_ShouldBeZeroByDefault()
    {
        // Arrange
        var localAuthority = new LocalAuthority
        {
            Id = Guid.NewGuid(),
            Code = "E09000001",
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.Equal(0u, localAuthority.Version);
    }

    [Fact]
    public void LocalAuthority_AllProperties_CanBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var localAuthority = new LocalAuthority
        {
            Id = id,
            Code = "E09000002",
            Name = "Local Authority Two",
            Version = 1
        };

        // Act & Assert
        Assert.Equal(id, localAuthority.Id);
        Assert.Equal("E09000002", localAuthority.Code);
        Assert.Equal("Local Authority Two", localAuthority.Name);
        Assert.Equal(1u, localAuthority.Version);
    }

    [Fact]
    public void LocalAuthority_Code_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var localAuthority = new LocalAuthority
        {
            Id = Guid.NewGuid(),
            Code = "E09000001",
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(localAuthority.Code));
    }

    [Fact]
    public void LocalAuthority_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var localAuthority = new LocalAuthority
        {
            Id = Guid.NewGuid(),
            Code = "E09000001",
            Name = "Local Authority One"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(localAuthority.Name));
    }
}