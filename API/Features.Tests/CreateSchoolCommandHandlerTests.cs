using Domain.Entities;
using Features.Schools.Commands.CreateSchool;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Features.Tests;

public class CreateSchoolCommandHandlerTests
{
    private async Task<ApplicationDbContext> GetContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateSchoolAndReturnId()
    {
        // Arrange
        await using var context = await GetContextAsync();
        var handler = new CreateSchoolCommandHandler(context);
        var command = new CreateSchoolCommand(
            Name: "Test School",
            URN: "TEST123",
            Address: "123 Test St",
            City: "Test City",
            County: null,
            Postcode: "12345",
            PhoneNumber: null,
            Website: null,
            Type: SchoolType.Primary,
            Capacity: 500,
            PupilsEnrolled: 450
        );

        // Act
        var result = await handler.ExecuteAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);

        var savedSchool = await context.Schools.FirstOrDefaultAsync(s => s.Id == 1, TestContext.Current.CancellationToken);
        Assert.NotNull(savedSchool);
        Assert.Equal("Test School", savedSchool!.Name);
        Assert.Equal("TEST123", savedSchool.URN);
    }

    [Fact]
    public async Task Handle_DuplicateURN_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await using var context = await GetContextAsync();
        var handler = new CreateSchoolCommandHandler(context);
        var command1 = new CreateSchoolCommand(
            Name: "School 1",
            URN: "DUPLICATE123",
            null, null, null, null, null, null,
            SchoolType.Primary,
            null, null
        );
        var command2 = new CreateSchoolCommand(
            Name: "School 2",
            URN: "DUPLICATE123",
            null, null, null, null, null, null,
            SchoolType.Secondary,
            null, null
        );

        // Act
        await handler.ExecuteAsync(command1, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.ExecuteAsync(command2, CancellationToken.None));
    }
}