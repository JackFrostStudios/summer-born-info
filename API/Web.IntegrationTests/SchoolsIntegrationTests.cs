using System.Net;
using System.Net.Http.Json;
using Domain.Entities;
using Features.Schools.Commands.CreateSchool;
using Features.Schools.Queries.GetAllSchools;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Web.IntegrationTests;

public class SchoolsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SchoolsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateSchool_ReturnsCreatedSchool_WithValidData()
    {
        // Arrange
        var command = new CreateSchoolCommand(
            "Test Primary School",
            "123456",
            "123 Test Street",
            "Test City",
            "Test County",
            "TE1 2ST",
            "01234567890",
            "https://testschool.example.com",
            SchoolType.Primary,
            300,
            250
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/schools", command);
        var content = await response.Content.ReadFromJsonAsync<CreateSchoolResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(content);
        Assert.True(content.Id > 0);
    }

    [Fact]
    public async Task GetAllSchools_ReturnsAllSchools()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/schools");
        var content = await response.Content.ReadFromJsonAsync<List<SchoolDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GetSchoolById_ReturnsSchool_WhenSchoolExists()
    {
        // Arrange
        var school = await SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync($"/api/schools/{school.Id}");
        var content = await response.Content.ReadFromJsonAsync<School>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(school.Id, content.Id);
        Assert.Equal(school.Name, content.Name);
    }

    [Fact]
    public async Task GetSchoolById_ReturnsNotFound_WhenSchoolDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/schools/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateSchool_ReturnsBadRequest_WhenCommandIsInvalid()
    {
        // Arrange
        var invalidCommand = new
        {
            Name = "", // Empty name should be invalid
            URN = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/schools", invalidCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<School> SeedTestDataAsync()
    {
        using var scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var school = new School
        {
            Name = "Seeded Test School",
            URN = "SEED001",
            Address = "Seed Address",
            City = "Seed City",
            County = "Seed County",
            Postcode = "SE1 1SE",
            PhoneNumber = "01234567890",
            Website = "https://seedschool.example.com",
            Type = SchoolType.Secondary,
            Capacity = 500,
            PupilsEnrolled = 450,
            CreatedAt = DateTime.UtcNow
        };

        db.Schools.Add(school);
        await db.SaveChangesAsync();

        return school;
    }

    private async Task CleanupTestDataAsync()
    {
        using var scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var testSchools = await db.Schools
            .Where(s => s.URN.StartsWith("SEED") || s.URN == "123456")
            .ToListAsync();

        if (testSchools.Any())
        {
            db.Schools.RemoveRange(testSchools);
            await db.SaveChangesAsync();
        }
    }
}