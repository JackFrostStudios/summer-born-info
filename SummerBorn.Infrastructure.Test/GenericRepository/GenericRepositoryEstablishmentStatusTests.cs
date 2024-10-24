using Microsoft.EntityFrameworkCore;
using SummerBorn.Core.Entity.Establishment;
using SummerBorn.Infrastructure.Test.DatabaseSetup;

namespace SummerBorn.Infrastructure.Test.GenericRepository;


[Collection(nameof(DatabaseCollection))]
public class GenericRepositoryEstablishmentStatusTests(DatabaseConfiguration DatabaseConfiguration)
{
    [Fact]
    public void GetAll_RetrieveEstablishmentStatus_AllExistingStatusesAreReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentStatus>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = genericRepository.GetAll().ToList();
        var seededEstablishmentStatus = result.Find(x => x.Id == DatabaseConfiguration.SeededData.EstablishmentStatus.Id);

        // Assert
        Assert.True(result.Count > 0);

        Assert.NotNull(seededEstablishmentStatus);
        Assert.Equivalent(DatabaseConfiguration.SeededData.EstablishmentStatus, seededEstablishmentStatus);
    }

    [Fact]
    public async Task GetById_RetrieveEstablishmentStatus_ExistingEstablishmentStatusIsReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentStatus>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = await genericRepository.GetById(DatabaseConfiguration.SeededData.EstablishmentStatus.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(DatabaseConfiguration.SeededData.EstablishmentStatus, result);
    }

    [Fact]
    public async Task Add_CreateEstablishmentStatus_CreatedEstablishmentStatusCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentStatus>(DatabaseConfiguration.SchoolContext);
        var newEstablishmentStatus = new EstablishmentStatus { Code = 1, Name = "New EstablishmentStatus" };

        // Act
        await genericRepository.Add(newEstablishmentStatus);
        var savedEstablishmentStatus = await DatabaseConfiguration.SchoolContext.EstablishmentStatus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newEstablishmentStatus.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, newEstablishmentStatus.Id);
        Assert.NotNull(savedEstablishmentStatus);
        Assert.Equivalent(newEstablishmentStatus, savedEstablishmentStatus);
    }

    [Fact]
    public async Task Update_UpdateEstablishmentStatus_UpdatedEstablishmentStatusCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentStatus>(DatabaseConfiguration.SchoolContext);
        var newEstablishmentStatus = new EstablishmentStatus { Code = 2, Name = "New EstablishmentStatus To Be Updated" };
        await genericRepository.Add(newEstablishmentStatus);

        // Act
        newEstablishmentStatus.Code = 3;
        newEstablishmentStatus.Name = "Updated EstablishmentStatus";
        await genericRepository.Update(newEstablishmentStatus);
        var savedEstablishmentStatus = await DatabaseConfiguration.SchoolContext.EstablishmentStatus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newEstablishmentStatus.Id);

        // Assert
        Assert.NotNull(savedEstablishmentStatus);
        Assert.Equal(3, savedEstablishmentStatus.Code);
        Assert.Equal("Updated EstablishmentStatus", savedEstablishmentStatus.Name);
        Assert.Equivalent(newEstablishmentStatus, savedEstablishmentStatus);
    }
}
