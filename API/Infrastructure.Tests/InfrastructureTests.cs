using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests;

public class InfrastructureTests
{
    [Fact]
    public async Task ApplicationDbContext_CanAddAndRetrieveSchool()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            URN = "TEST123",
            Address = "123 Test St",
            City = "Test City",
            Type = Domain.Entities.SchoolType.Primary
        };

        // Act
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        // Assert
        var retrievedSchool = await context.Schools.FirstOrDefaultAsync(s => s.Id == 1);
        Assert.NotNull(retrievedSchool);
        Assert.Equal("Test School", retrievedSchool!.Name);
        Assert.Equal("TEST123", retrievedSchool.URN);
    }

    [Fact]
    public async Task ApplicationDbContext_SchoolCount_ShouldBeCorrect()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.Schools.Add(new School
        {
            Id = 1,
            Name = "School 1",
            URN = "SCH001"
        });
        context.Schools.Add(new School
        {
            Id = 2,
            Name = "School 2",
            URN = "SCH002"
        });
        await context.SaveChangesAsync();

        // Act
        var count = await context.Schools.CountAsync();

        // Assert
        Assert.Equal(2, count);
    }
}