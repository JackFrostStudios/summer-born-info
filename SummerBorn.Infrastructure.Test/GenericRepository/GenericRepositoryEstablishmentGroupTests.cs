using Microsoft.EntityFrameworkCore;
using SummerBorn.Core.Entity.Establishment;
using SummerBorn.Infrastructure.Test.DatabaseSetup;

namespace SummerBorn.Infrastructure.Test.GenericRepository;


[Collection(nameof(DatabaseCollection))]
public class GenericRepositoryEstablishmentGroupTests(DatabaseConfiguration DatabaseConfiguration)
{
    [Fact]
    public void GetAll_RetrieveEstablishmentGroup_AllExistingGroupsAreReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentGroup>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = genericRepository.GetAll().ToList();
        var seededEstablishmentGroup = result.Find(x => x.Id == DatabaseConfiguration.SeededData.EstablishmentGroup.Id);

        // Assert
        Assert.True(result.Count > 0);

        Assert.NotNull(seededEstablishmentGroup);
        Assert.Equivalent(DatabaseConfiguration.SeededData.EstablishmentGroup, seededEstablishmentGroup);
    }

    [Fact]
    public async Task GetById_RetrieveEstablishmentGroup_ExistingEstablishmentGroupIsReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentGroup>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = await genericRepository.GetById(DatabaseConfiguration.SeededData.EstablishmentGroup.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(DatabaseConfiguration.SeededData.EstablishmentGroup, result);
    }

    [Fact]
    public async Task Add_CreateEstablishmentGroup_CreatedEstablishmentGroupCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentGroup>(DatabaseConfiguration.SchoolContext);
        var newEstablishmentGroup = new EstablishmentGroup { Code = 1, Name = "New EstablishmentGroup" };

        // Act
        await genericRepository.Add(newEstablishmentGroup);
        var savedEstablishmentGroup = await DatabaseConfiguration.SchoolContext.EstablishmentGroup.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newEstablishmentGroup.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, newEstablishmentGroup.Id);
        Assert.NotNull(savedEstablishmentGroup);
        Assert.Equivalent(newEstablishmentGroup, savedEstablishmentGroup);
    }

    [Fact]
    public async Task Update_UpdateEstablishmentGroup_UpdatedEstablishmentGroupCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<EstablishmentGroup>(DatabaseConfiguration.SchoolContext);
        var newEstablishmentGroup = new EstablishmentGroup { Code = 2, Name = "New EstablishmentGroup To Be Updated" };
        await genericRepository.Add(newEstablishmentGroup);

        // Act
        newEstablishmentGroup.Code = 3;
        newEstablishmentGroup.Name = "Updated EstablishmentGroup";
        await genericRepository.Update(newEstablishmentGroup);
        var savedEstablishmentGroup = await DatabaseConfiguration.SchoolContext.EstablishmentGroup.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newEstablishmentGroup.Id);

        // Assert
        Assert.NotNull(savedEstablishmentGroup);
        Assert.Equal(3, savedEstablishmentGroup.Code);
        Assert.Equal("Updated EstablishmentGroup", savedEstablishmentGroup.Name);
        Assert.Equivalent(newEstablishmentGroup, savedEstablishmentGroup);
    }
}
