namespace SummerBornInfo.Web.Features.SchoolCommands.Upsert;

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
        var school = await context.School.FirstOrDefaultAsync(s => s.URN == req.URN, c);
        if (school != null)
        {
            school = await UpdateExistingSchool(school, req, c);
        }
        else
        {
            school = await MapToEntity(req, c);
            context.Add(school);
        }

        await context.SaveChangesAsync(c);

        var resp = Map.FromEntity(school);

        await SendAsync(response: resp, cancellation: c);
    }

    private async Task<PhaseOfEducation?> GetPhaseOfEducation(Request r, CancellationToken c)
    {
        var phaseOfEducation = await context
            .PhaseOfEducation
            .FirstOrDefaultAsync(e => e.Id == r.PhaseOfEducationId, c);

        if (phaseOfEducation == null)
        {
            AddError(r => r.PhaseOfEducationId, "Phase of Education must exist");
        }
        return phaseOfEducation;
    }

    private async Task<LocalAuthority?> GetLocalAuthority(Request r, CancellationToken c)
    {
        var localAuthority = await context
            .LocalAuthority
            .FirstOrDefaultAsync(e => e.Id == r.LocalAuthorityId, c);

        if (localAuthority == null)
        {
            AddError(r => r.LocalAuthorityId, "Local Authority must exist");
        }
        return localAuthority;
    }

    private async Task<EstablishmentType?> GetEstablishmentType(Request r, CancellationToken c)
    {
        var establishmentType = await context
            .EstablishmentType
            .FirstOrDefaultAsync(e => e.Id == r.EstablishmentTypeId, c);

        if (establishmentType == null)
        {
            AddError(r => r.EstablishmentTypeId, "Establishment Type must exist");
        }

        return establishmentType;
    }

    private async Task<EstablishmentGroup?> GetEstablishmentGroup(Request r, CancellationToken c)
    {
        var establishmentGroup = await context
            .EstablishmentGroup
            .FirstOrDefaultAsync(e => e.Id == r.EstablishmentGroupId, c);

        if (establishmentGroup == null)
        {
            AddError(r => r.EstablishmentGroupId, "Establishment Group must exist");
        }

        return establishmentGroup;
    }

    private async Task<EstablishmentStatus?> GetEstablishmentStatus(Request r, CancellationToken c)
    {
        var establishmentStatus = await context
            .EstablishmentStatus
            .FirstOrDefaultAsync(e => e.Id == r.EstablishmentStatusId, c);

        if (establishmentStatus == null)
        {
            AddError(r => r.EstablishmentStatusId, "Establishment Status must exist");
        }

        return establishmentStatus;
    }

    private static SchoolAddress GetAddress(Request r)
    {
        return new SchoolAddress
        {
            Street = r.Address.Street,
            Locality = r.Address.Locality,
            AddressThree = r.Address.AddressThree,
            Town = r.Address.Town,
            County = r.Address.County,
            PostCode = r.Address.PostCode,
        };
    }
    private async Task<School> MapToEntity(Request r, CancellationToken c)
    {
        var phaseOfEducation = await GetPhaseOfEducation(r, c);
        var localAuthority = await GetLocalAuthority(r, c);
        var establishmentType = await GetEstablishmentType(r, c);
        var establishmentGroup = await GetEstablishmentGroup(r, c);
        var establishmentStatus = await GetEstablishmentStatus(r, c);
        ThrowIfAnyErrors();

        return new School
        {
            URN = r.URN,
            UKPRN = r.UKPRN,
            EstablishmentNumber = r.EstablishmentNumber,
            Name = r.Name,
            Address = GetAddress(r),
            OpenDate = r.OpenDate,
            CloseDate = r.CloseDate,
            PhaseOfEducation = phaseOfEducation!,
            LocalAuthority = localAuthority!,
            EstablishmentType = establishmentType!,
            EstablishmentGroup = establishmentGroup!,
            EstablishmentStatus = establishmentStatus!,
        };
    }

    private async Task<School> UpdateExistingSchool(School school, Request r, CancellationToken c)
    {
        if (school != null && school.UKPRN != r.UKPRN)
        {
            AddError(r => r.UKPRN, $"UKPRN does not match existing record with URN {r.URN}");
        }

        if (school != null && school.EstablishmentNumber != r.EstablishmentNumber)
        {
            AddError(r => r.EstablishmentNumber, $"Establishment Number does not match existing record with URN {r.URN}");
        }

        ThrowIfAnyErrors();

        var phaseOfEducation = await GetPhaseOfEducation(r, c);
        var localAuthority = await GetLocalAuthority(r, c);
        var establishmentType = await GetEstablishmentType(r, c);
        var establishmentGroup = await GetEstablishmentGroup(r, c);
        var establishmentStatus = await GetEstablishmentStatus(r, c);
        ThrowIfAnyErrors();

        if (school != null)
        {
            school.Name = r.Name;
            school.Address = GetAddress(r);
            school.OpenDate = r.OpenDate;
            school.CloseDate = r.CloseDate;
            school.PhaseOfEducation = phaseOfEducation!;
            school.LocalAuthority = localAuthority!;
            school.EstablishmentType = establishmentType!;
            school.EstablishmentGroup = establishmentGroup!;
            school.EstablishmentStatus = establishmentStatus!;

        }
        return school!;

    }
}