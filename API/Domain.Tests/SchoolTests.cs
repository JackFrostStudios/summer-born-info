using Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class SchoolTests
{
    [Fact]
    public void School_Name_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            URN = "TEST123",
            Address = "123 Test St"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(school.Name));
    }

    [Fact]
    public void School_URN_ShouldNotBeNullOrEmpty()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            URN = "TEST123"
        };

        // Act & Assert
        Assert.False(string.IsNullOrEmpty(school.URN));
    }

    [Fact]
    public void School_ShouldHaveValidType()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            URN = "TEST123",
            Type = SchoolType.Primary
        };

        // Act & Assert
        Assert.Equal(SchoolType.Primary, school.Type);
    }
}