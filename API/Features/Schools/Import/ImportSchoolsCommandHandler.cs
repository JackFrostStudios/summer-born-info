namespace SummerBornInfo.Features.Schools.Import;

public class ImportSchoolsCommandHandler
{
    private readonly ApplicationDbContext _context;

    public ImportSchoolsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportSchoolsResponse> ExecuteAsync(ImportSchoolsCommand command, CancellationToken cancellationToken)
    {
        var csvStream = command.CsvStream;

        if (csvStream == null)
        {
            throw new ArgumentException("CSV stream is required");
        }

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

        // Read header to validate CSV structure
        csv.Read();
        csv.ReadHeader();

        var headers = csv.HeaderRecord;
        if (headers == null || !headers.Contains("URN"))
        {
            throw new ArgumentException("Invalid CSV format: Missing required 'URN' column");
        }

        var totalSchools = 0;
        var schoolsCreated = 0;
        var schoolsUpdated = 0;
        var errors = 0;

        while (await csv.ReadAsync())
        {
            totalSchools++;

            try
            {
                var record = csv.GetRecord<dynamic>();

                // Parse URN
                if (!int.TryParse(record.URN, out int urn))
                {
                    errors++;
                    continue;
                }

                // Check if school exists
                var existingSchool = await _context.Schools
                    .Include(s => s.Address)
                    .Include(s => s.LocalAuthority)
                    .Include(s => s.PhaseOfEducation)
                    .Include(s => s.EstablishmentType)
                    .Include(s => s.EstablishmentGroup)
                    .Include(s => s.EstablishmentStatus)
                    .FirstOrDefaultAsync(s => s.URN == urn, cancellationToken);

                // Get or create LocalAuthority
                var laCode = record["LA (code)"]?.ToString() ?? "";
                var localAuthority = await GetOrCreateLocalAuthority(laCode, record["LA (name)"]?.ToString() ?? "", cancellationToken);

                // Get or create PhaseOfEducation
                var phaseCode = record["PhaseOfEducation (code)"]?.ToString() ?? "";
                var phaseOfEducation = await GetOrCreatePhaseOfEducation(phaseCode, record["PhaseOfEducation (name)"]?.ToString() ?? "", cancellationToken);

                // Get or create EstablishmentType
                var typeCode = record["TypeOfEstablishment (code)"]?.ToString() ?? "";
                var establishmentType = await GetOrCreateEstablishmentType(typeCode, record["TypeOfEstablishment (name)"]?.ToString() ?? "", cancellationToken);

                // Get or create EstablishmentGroup
                var groupCode = record["EstablishmentTypeGroup (code)"]?.ToString() ?? "";
                var establishmentGroup = await GetOrCreateEstablishmentGroup(groupCode, record["EstablishmentTypeGroup (name)"]?.ToString() ?? "", cancellationToken);

                // Get or create EstablishmentStatus
                var statusCode = record["EstablishmentStatus (code)"]?.ToString() ?? "";
                var establishmentStatus = await GetOrCreateEstablishmentStatus(statusCode, record["EstablishmentStatus (name)"]?.ToString() ?? "", cancellationToken);

                // Parse dates
                DateOnly? openDate = null;
                if (!string.IsNullOrEmpty(record.OpenDate))
                {
                    if (DateOnly.TryParse(record.OpenDate, out DateOnly parsedOpenDate))
                    {
                        openDate = parsedOpenDate;
                    }
                }

                DateOnly? closeDate = null;
                if (!string.IsNullOrEmpty(record.CloseDate))
                {
                    if (DateOnly.TryParse(record.CloseDate, out DateOnly parsedCloseDate))
                    {
                        closeDate = parsedCloseDate;
                    }
                }

                // Parse EstablishmentNumber
                if (!int.TryParse(record.EstablishmentNumber, out int establishmentNumber))
                {
                    errors++;
                    continue;
                }

                // Parse UKPRN (optional)
                int? ukprn = null;
                if (!string.IsNullOrEmpty(record.UKPRN))
                {
                    if (int.TryParse(record.UKPRN, out int parsedUkprn))
                    {
                        ukprn = parsedUkprn;
                    }
                }

                // Create or update school
                if (existingSchool == null)
                {
                    var school = new School
                    {
                        Id = Guid.NewGuid(),
                        URN = urn,
                        UKPRN = ukprn,
                        EstablishmentNumber = establishmentNumber,
                        Name = record.EstablishmentName?.ToString() ?? "",
                        Address = new SchoolAddress
                        {
                            SchoolId = Guid.NewGuid(),
                            Street = record.Street?.ToString(),
                            Locality = record.Locality?.ToString(),
                            AddressThree = record.Address3?.ToString(),
                            Town = record.Town?.ToString() ?? "",
                            County = record["County (name)"]?.ToString(),
                            PostCode = record.Postcode?.ToString() ?? "",
                            Version = 0
                        },
                        OpenDate = openDate,
                        CloseDate = closeDate,
                        PhaseOfEducationId = phaseOfEducation.Id,
                        PhaseOfEducation = phaseOfEducation,
                        LocalAuthorityId = localAuthority.Id,
                        LocalAuthority = localAuthority,
                        EstablishmentTypeId = establishmentType.Id,
                        EstablishmentType = establishmentType,
                        EstablishmentGroupId = establishmentGroup.Id,
                        EstablishmentGroup = establishmentGroup,
                        EstablishmentStatusId = establishmentStatus.Id,
                        EstablishmentStatus = establishmentStatus,
                        Version = 0
                    };

                    _context.Schools.Add(school);
                    schoolsCreated++;
                }
                else
                {
                    // Update existing school
                    existingSchool.Name = record.EstablishmentName?.ToString() ?? existingSchool.Name;
                    existingSchool.UKPRN = ukprn;
                    existingSchool.EstablishmentNumber = establishmentNumber;
                    existingSchool.OpenDate = openDate;
                    existingSchool.CloseDate = closeDate;
                    existingSchool.PhaseOfEducationId = phaseOfEducation.Id;
                    existingSchool.PhaseOfEducation = phaseOfEducation;
                    existingSchool.LocalAuthorityId = localAuthority.Id;
                    existingSchool.LocalAuthority = localAuthority;
                    existingSchool.EstablishmentTypeId = establishmentType.Id;
                    existingSchool.EstablishmentType = establishmentType;
                    existingSchool.EstablishmentGroupId = establishmentGroup.Id;
                    existingSchool.EstablishmentGroup = establishmentGroup;
                    existingSchool.EstablishmentStatusId = establishmentStatus.Id;
                    existingSchool.EstablishmentStatus = establishmentStatus;

                    // Update address
                    if (existingSchool.Address != null)
                    {
                        existingSchool.Address.Street = record.Street?.ToString() ?? existingSchool.Address.Street;
                        existingSchool.Address.Locality = record.Locality?.ToString() ?? existingSchool.Address.Locality;
                        existingSchool.Address.AddressThree = record.Address3?.ToString() ?? existingSchool.Address.AddressThree;
                        existingSchool.Address.Town = record.Town?.ToString() ?? existingSchool.Address.Town;
                        existingSchool.Address.County = record["County (name)"]?.ToString() ?? existingSchool.Address.County;
                        existingSchool.Address.PostCode = record.Postcode?.ToString() ?? existingSchool.Address.PostCode;
                    }

                    schoolsUpdated++;
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error and continue
                errors++;
                Console.WriteLine($"Error processing school: {ex.Message}");
            }
        }

        return new ImportSchoolsResponse(
            totalSchools,
            schoolsCreated,
            schoolsUpdated,
            errors
        );
    }

    private async Task<LocalAuthority> GetOrCreateLocalAuthority(string code, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Local Authority code is required");

        var existing = await _context.LocalAuthorities
            .FirstOrDefaultAsync(la => la.Code == code, cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var localAuthority = new LocalAuthority
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Version = 0
        };

        _context.LocalAuthorities.Add(localAuthority);
        return localAuthority;
    }

    private async Task<PhaseOfEducation> GetOrCreatePhaseOfEducation(string code, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Phase of Education code is required");

        var existing = await _context.PhasesOfEducation
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var phase = new PhaseOfEducation
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Version = 0
        };

        _context.PhasesOfEducation.Add(phase);
        return phase;
    }

    private async Task<EstablishmentType> GetOrCreateEstablishmentType(string code, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Establishment Type code is required");

        var existing = await _context.EstablishmentTypes
            .FirstOrDefaultAsync(et => et.Code == code, cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var type = new EstablishmentType
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Version = 0
        };

        _context.EstablishmentTypes.Add(type);
        return type;
    }

    private async Task<EstablishmentGroup> GetOrCreateEstablishmentGroup(string code, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Establishment Group code is required");

        var existing = await _context.EstablishmentGroups
            .FirstOrDefaultAsync(eg => eg.Code == code, cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var group = new EstablishmentGroup
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Version = 0
        };

        _context.EstablishmentGroups.Add(group);
        return group;
    }

    private async Task<EstablishmentStatus> GetOrCreateEstablishmentStatus(string code, string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Establishment Status code is required");

        var existing = await _context.EstablishmentStatuses
            .FirstOrDefaultAsync(es => es.Code == code, cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var status = new EstablishmentStatus
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Version = 0
        };

        _context.EstablishmentStatuses.Add(status);
        return status;
    }
}