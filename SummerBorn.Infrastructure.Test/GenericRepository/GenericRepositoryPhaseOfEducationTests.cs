using Microsoft.EntityFrameworkCore;
using SummerBorn.Core.Entity;
using SummerBorn.Infrastructure.Test.DatabaseSetup;

namespace SummerBorn.Infrastructure.Test.GenericRepository;


[Collection(nameof(DatabaseCollection))]
public class GenericRepositoryPhaseOfEducationTests(DatabaseConfiguration DatabaseConfiguration)
{
    [Fact]
    public void GetAll_RetrievePhaseOfEducation_AllExistingPhasesAreReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<PhaseOfEducation>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = genericRepository.GetAll().ToList();
        var seededPhase = result.Find(x => x.Id == DatabaseConfiguration.SeededData.PhaseOfEducation.Id);

        // Assert
        Assert.True(result.Count > 0);

        Assert.NotNull(seededPhase);
        Assert.Equivalent(DatabaseConfiguration.SeededData.PhaseOfEducation, seededPhase);
    }

    [Fact]
    public async Task GetById_RetrievePhaseOfEducation_ExistingPhaseIsReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<PhaseOfEducation>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = await genericRepository.GetById(DatabaseConfiguration.SeededData.PhaseOfEducation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(DatabaseConfiguration.SeededData.PhaseOfEducation, result);
    }

    [Fact]
    public async Task Add_CreatePhaseOfEducation_CreatedPhaseCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<PhaseOfEducation>(DatabaseConfiguration.SchoolContext);
        var newPhase = new PhaseOfEducation { Code = 1, Name = "New Phase" };

        // Act
        await genericRepository.Add(newPhase);
        var savedPhase = await DatabaseConfiguration.SchoolContext.PhaseOfEducation.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newPhase.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, newPhase.Id);
        Assert.NotNull(savedPhase);
        Assert.Equivalent(newPhase, savedPhase);
    }

    [Fact]
    public async Task Update_UpdatePhaseOfEducation_UpdatedPhaseCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<PhaseOfEducation>(DatabaseConfiguration.SchoolContext);
        var newPhase = new PhaseOfEducation { Code = 2, Name = "New Phase To Be Updated" };
        await genericRepository.Add(newPhase);

        // Act
        newPhase.Code = 3;
        newPhase.Name = "Updated Phase";
        await genericRepository.Update(newPhase);
        var savedPhase = await DatabaseConfiguration.SchoolContext.PhaseOfEducation.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newPhase.Id);

        // Assert
        Assert.NotNull(savedPhase);
        Assert.Equal(3, savedPhase.Code);
        Assert.Equal("Updated Phase", savedPhase.Name);
        Assert.Equivalent(newPhase, savedPhase);
    }
}
