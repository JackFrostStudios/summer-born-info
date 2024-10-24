using Microsoft.EntityFrameworkCore;
using SummerBorn.Core.Entity;
using SummerBorn.Infrastructure.Test.DatabaseSetup;

namespace SummerBorn.Infrastructure.Test.GenericRepository;


[Collection(nameof(DatabaseCollection))]
public class GenericRepositoryLocalAuthorityTests(DatabaseConfiguration DatabaseConfiguration)
{
    [Fact]
    public void GetAll_RetrieveLocalAuthority_AllExistingAuthoritiesAreReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<LocalAuthority>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = genericRepository.GetAll().ToList();
        var seededAuthority = result.Find(x => x.Id == DatabaseConfiguration.SeededData.LocalAuthority.Id);

        // Assert
        Assert.True(result.Count > 0);

        Assert.NotNull(seededAuthority);
        Assert.Equivalent(DatabaseConfiguration.SeededData.LocalAuthority, seededAuthority);
    }

    [Fact]
    public async Task GetById_RetrieveLocalAuthority_ExistingAuthorityIsReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<LocalAuthority>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = await genericRepository.GetById(DatabaseConfiguration.SeededData.LocalAuthority.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(DatabaseConfiguration.SeededData.LocalAuthority, result);
    }

    [Fact]
    public async Task Add_CreateLocalAuthority_CreatedAuthorityCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<LocalAuthority>(DatabaseConfiguration.SchoolContext);
        var newAuthority = new LocalAuthority { Code = 1, Name = "New Authority" };

        // Act
        await genericRepository.Add(newAuthority);
        var savedAuthority = await DatabaseConfiguration.SchoolContext.LocalAuthority.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newAuthority.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, newAuthority.Id);
        Assert.NotNull(savedAuthority);
        Assert.Equivalent(newAuthority, savedAuthority);
    }

    [Fact]
    public async Task Update_UpdateLocalAuthority_UpdatedAuthorityCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<LocalAuthority>(DatabaseConfiguration.SchoolContext);
        var newAuthority = new LocalAuthority { Code = 2, Name = "New Authority To Be Updated" };
        await genericRepository.Add(newAuthority);

        // Act
        newAuthority.Code = 3;
        newAuthority.Name = "Updated Authority";
        await genericRepository.Update(newAuthority);
        var savedAuthority = await DatabaseConfiguration.SchoolContext.LocalAuthority.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newAuthority.Id);

        // Assert
        Assert.NotNull(savedAuthority);
        Assert.Equal(3, savedAuthority.Code);
        Assert.Equal("Updated Authority", savedAuthority.Name);
        Assert.Equivalent(newAuthority, savedAuthority);
    }
}
