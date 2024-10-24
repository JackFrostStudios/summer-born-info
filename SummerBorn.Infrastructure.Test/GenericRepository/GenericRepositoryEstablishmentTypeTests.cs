using Microsoft.EntityFrameworkCore;
using SummerBorn.Core.Entity.Establishment;
using SummerBorn.Infrastructure.Test.DatabaseSetup;

namespace SummerBorn.Infrastructure.Test.GenericRepository;


[Collection(nameof(DatabaseCollection))]
public class GenericRepositoryEstablishmentTypeTests(DatabaseConfiguration DatabaseConfiguration)
{
    [Fact]
    public void GetAll_RetrieveEstablishmentType_AllExistingTypesAreReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentType>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = genericRepository.GetAll().ToList();
        var seededEstablishmentType = result.Find(x => x.Id == DatabaseConfiguration.SeededData.EstablishmentType.Id);

        // Assert
        Assert.True(result.Count > 0);

        Assert.NotNull(seededEstablishmentType);
        Assert.Equivalent(DatabaseConfiguration.SeededData.EstablishmentType, seededEstablishmentType);
    }

    [Fact]
    public async Task GetById_RetrieveEstablishmentType_ExistingEstablishmentTypeIsReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentType>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = await genericRepository.GetById(DatabaseConfiguration.SeededData.EstablishmentType.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(DatabaseConfiguration.SeededData.EstablishmentType, result);
    }

    [Fact]
    public async Task Add_CreateEstablishmentType_CreatedEstablishmentTypeCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentType>(DatabaseConfiguration.SchoolContext);
        var newEstablishmentType = new EstablishmentType { Code = 1, Name = "New EstablishmentType" };

        // Act
        await genericRepository.Add(newEstablishmentType);
        var savedEstablishmentType = await DatabaseConfiguration.SchoolContext.EstablishmentType.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newEstablishmentType.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, newEstablishmentType.Id);
        Assert.NotNull(savedEstablishmentType);
        Assert.Equivalent(newEstablishmentType, savedEstablishmentType);
    }

    [Fact]
    public async Task Update_UpdateEstablishmentType_UpdatedEstablishmentTypeCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentType>(DatabaseConfiguration.SchoolContext);
        var newEstablishmentType = new EstablishmentType { Code = 2, Name = "New EstablishmentType To Be Updated" };
        await genericRepository.Add(newEstablishmentType);

        // Act
        newEstablishmentType.Code = 3;
        newEstablishmentType.Name = "Updated EstablishmentType";
        await genericRepository.Update(newEstablishmentType);
        var savedEstablishmentType = await DatabaseConfiguration.SchoolContext.EstablishmentType.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newEstablishmentType.Id);

        // Assert
        Assert.NotNull(savedEstablishmentType);
        Assert.Equal(3, savedEstablishmentType.Code);
        Assert.Equal("Updated EstablishmentType", savedEstablishmentType.Name);
        Assert.Equivalent(newEstablishmentType, savedEstablishmentType);
    }
}
