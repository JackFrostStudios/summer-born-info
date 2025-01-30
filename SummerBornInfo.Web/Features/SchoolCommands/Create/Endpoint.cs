namespace SummerBornInfo.Web.Features.SchoolCommands.Create;

internal sealed class Endpoint(SchoolContext context) : Endpoint<Request, Response, Mapper>
{
    private readonly SchoolContext context = context;

    public override void Configure()
    {
        AllowAnonymous();
        Post("school");
    }

    public override async Task HandleAsync(Request req, CancellationToken c)
    {
        var school = await MapToEntity(req, c);

        context.Add(school);
        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(school);

        await SendAsync(response: resp, cancellation: c);
    }

    public async Task<School> MapToEntity(Request r, CancellationToken c)
    {
        var phaseOfEducation = await context
            .PhaseOfEducation
            .FirstOrDefaultAsync(e => e.Id == r.PhaseOfEducationId, c);

        if (phaseOfEducation == null)
        {
            AddError(r => r.PhaseOfEducationId, "Phase of Education must exist");
        }

        var localAuthority = await context
            .LocalAuthority
            .FirstOrDefaultAsync(e => e.Id == r.LocalAuthorityId, c);

        if (localAuthority == null)
        {
            AddError(r => r.LocalAuthorityId, "Local Authority must exist");
        }

        var establishmentType = await context
            .EstablishmentType
            .FirstOrDefaultAsync(e => e.Id == r.EstablishmentTypeId, c);

        if (establishmentType == null)
        {
            AddError(r => r.EstablishmentTypeId, "Establishment Type must exist");
        }

        var establishmentGroup = await context
            .EstablishmentGroup
            .FirstOrDefaultAsync(e => e.Id == r.EstablishmentGroupId, c);

        if (establishmentGroup == null)
        {
            AddError(r => r.EstablishmentGroupId, "Establishment Group must exist");
        }

        var establishmentStatus = await context
            .EstablishmentStatus
            .FirstOrDefaultAsync(e => e.Id == r.EstablishmentStatusId, c);

        if (establishmentStatus == null)
        {
            AddError(r => r.EstablishmentStatusId, "Establishment Status must exist");
        }

        ThrowIfAnyErrors();

        var address = new Address
        {
            Street = r.Address.Street,
            Locality = r.Address.Locality,
            AddressThree = r.Address.AddressThree,
            Town = r.Address.Town,
            County = r.Address.County,
            PostCode = r.Address.PostCode,
        };
        return new School
        {
            URN = r.URN,
            UKPRN = r.UKPRN,
            EstablishmentNumber = r.EstablishmentNumber,
            Name = r.Name,
            Address = address,
            OpenDate = r.OpenDate,
            CloseDate = r.CloseDate,
            PhaseOfEducation = phaseOfEducation!,
            LocalAuthority = localAuthority!,
            EstablishmentType = establishmentType!,
            EstablishmentGroup = establishmentGroup!,
            EstablishmentStatus = establishmentStatus!,
        };
    }
}