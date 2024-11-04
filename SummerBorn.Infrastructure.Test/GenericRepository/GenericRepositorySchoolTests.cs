using Microsoft.EntityFrameworkCore;
using SummerBorn.Core.Entity.School;
using SummerBorn.Infrastructure.Test.DatabaseSetup;

namespace SummerBorn.Infrastructure.Test.GenericRepository;


[Collection(nameof(DatabaseCollection))]
public class GenericRepositorySchoolTests(DatabaseConfiguration DatabaseConfiguration)
{
    [Fact]
    public void GetAll_RetrieveSchool_AllExistingSchoolsAreReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<School>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = genericRepository.GetAll().ToList();
        var seededSchool = result.Find(x => x.Id == DatabaseConfiguration.SeededData.School.Id);

        // Assert
        Assert.True(result.Count > 0);

        Assert.NotNull(seededSchool);
        Assert.Equivalent(DatabaseConfiguration.SeededData.School, seededSchool);
    }

    [Fact]
    public async Task GetById_RetrieveSchool_ExistingSchoolIsReturned()
    {
        // Arrange
        var genericRepository = new GenericRepository<School>(DatabaseConfiguration.SchoolContext);

        // Act
        var result = await genericRepository.GetById(DatabaseConfiguration.SeededData.School.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(DatabaseConfiguration.SeededData.School, result);
    }

    [Fact]
    public async Task Add_CreateSchool_CreatedSchoolCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<School>(DatabaseConfiguration.SchoolContext);
        var newSchool = new School
        {
            Address = new Address { Street = "new Str", Locality = "new loc", AddressThree = "new addr3", Town = "new twn", County = "new Coun", PostCode = "new PT12CD" },
            CloseDate = new DateOnly(2025, 10, 21),
            EstablishmentGroup = DatabaseConfiguration.SeededData.EstablishmentGroup,
            EstablishmentNumber = 1,
            EstablishmentStatus = DatabaseConfiguration.SeededData.EstablishmentStatus,
            EstablishmentType = DatabaseConfiguration.SeededData.EstablishmentType,
            LocalAuthority = DatabaseConfiguration.SeededData.LocalAuthority,
            Name = "New Local Primary School",
            OpenDate = new DateOnly(2022, 10, 21),
            PhaseOfEducation = DatabaseConfiguration.SeededData.PhaseOfEducation,
            UKPRN = 2,
            URN = 3
        };

        // Act
        await genericRepository.Add(newSchool);
        var savedSchool = await DatabaseConfiguration.SchoolContext.School
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == newSchool.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, newSchool.Id);
        Assert.NotNull(savedSchool);
        Assert.Equivalent(newSchool, savedSchool);
    }

    [Fact]
    public async Task Update_UpdateSchool_UpdatedSchoolCanBeRetrieved()
    {
        // Arrange
        var genericRepository = new GenericRepository<School>(DatabaseConfiguration.SchoolContext);
        var newSchool = new School
        {
            Address = new Address { Street = "new Str to be updated", Locality = "new loc to be updated", AddressThree = "new addr3 to be updated", Town = "new twn to be updated", County = "new Coun to be updated", PostCode = "new PT12CD to be updated" },
            CloseDate = new DateOnly(2026, 10, 21),
            EstablishmentGroup = DatabaseConfiguration.SeededData.EstablishmentGroup,
            EstablishmentNumber = 2,
            EstablishmentStatus = DatabaseConfiguration.SeededData.EstablishmentStatus,
            EstablishmentType = DatabaseConfiguration.SeededData.EstablishmentType,
            LocalAuthority = DatabaseConfiguration.SeededData.LocalAuthority,
            Name = "New Local Primary School to be updated",
            OpenDate = new DateOnly(2021, 10, 21),
            PhaseOfEducation = DatabaseConfiguration.SeededData.PhaseOfEducation,
            UKPRN = 3,
            URN = 4
        };
        await genericRepository.Add(newSchool);

        // Act
        var updatedAddress = new Address { SchoolId = newSchool.Id, Street = "new Str updated", Locality = "new loc updated", AddressThree = "new addr3 updated", Town = "new twn updated", County = "new Coun updated", PostCode = "new PT12CD updated" };
        newSchool.Address = updatedAddress;
        newSchool.CloseDate = new DateOnly(2027, 10, 21);
        newSchool.EstablishmentNumber = 3;
        newSchool.Name = "New Local Primary School updated";
        newSchool.OpenDate = new DateOnly(2020, 10, 21);
        newSchool.UKPRN = 4;
        newSchool.URN = 5;
        await genericRepository.Update(newSchool);
        
        var savedSchool = await DatabaseConfiguration.SchoolContext.School.AsNoTracking().FirstOrDefaultAsync(x => x.Id == newSchool.Id);

        // Assert
        Assert.NotNull(savedSchool);
        Assert.Equivalent(updatedAddress, savedSchool.Address);
        Assert.Equal(new DateOnly(2027, 10, 21), savedSchool.CloseDate);
        Assert.Equal(3, savedSchool.EstablishmentNumber);
        Assert.Equal("New Local Primary School updated", savedSchool.Name);
        Assert.Equal(new DateOnly(2020, 10, 21), savedSchool.OpenDate);
        Assert.Equal(4, savedSchool.UKPRN);
        Assert.Equal(5, savedSchool.URN);
        Assert.Equivalent(newSchool, savedSchool);
    }
}
