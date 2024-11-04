using SummerBorn.Core.Commands;
using SummerBorn.Core.Query.Establishment;

namespace SummerBorn.Core.Services.Establishment;
public class EstablishmentGroupService(IRepository<EstablishmentGroup> repository)
{
    public async Task<Guid> CreateEstablishmentGroup(CreateBasicLookupEntity createCommand)
    {
        var establishmentGroup = new EstablishmentGroup { Code = createCommand.Code, Name = createCommand.Name };
        await repository.Add(establishmentGroup);
        return establishmentGroup.Id;
    }

    public async Task<EstablishmentGroupDto?> FindById(Guid id)
    {
        var group = await repository.GetById(id);
        return group == null ? null : new EstablishmentGroupDto(group.Id, group.Code, group.Name);
    }

    public IEnumerable<EstablishmentGroupDto> FindAll()
    {
        return repository.GetAll().ToList().Select(g => new EstablishmentGroupDto(g.Id, g.Code, g.Name));
    }
}
