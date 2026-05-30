namespace SummerBornInfo.Features.Schools.Queries.GetNearbySchools;

public sealed class GetNearbySchoolsQueryHandler(ApplicationDbContext context)
{
    private const double MetersPerMile = 1609.344d;
    private const string NearbySearchSql =
        "with nearby as (" +
        "    select " +
        "        s.\"Id\", " +
        "        ST_Distance( " +
        "            s.\"Location\", " +
        "            ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography) as \"DistanceMeters\" " +
        "    from school as s " +
        "    where " +
        "        s.\"Location\" is not null " +
        "        and ST_DWithin( " +
        "            s.\"Location\", " +
        "            ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography, " +
        "            @radiusMeters)) " +
        "select n.\"Id\", n.\"DistanceMeters\" " +
        "from nearby as n " +
        "where " +
        "    not @hasCursor " +
        "    or n.\"DistanceMeters\" > @cursorDistanceMeters " +
        "    or (n.\"DistanceMeters\" = @cursorDistanceMeters and n.\"Id\" > @cursorSchoolId) " +
        "order by " +
        "    n.\"DistanceMeters\" asc, " +
        "    n.\"Id\" asc " +
        "limit @limit;";

    private readonly ApplicationDbContext _context = context;

    public async Task<SchoolsResponse> ExecuteAsync(GetNearbySchoolsRequest request, CancellationToken cancellationToken)
    {
        var validatedRequest = ValidateRequest(request);
        var pageSize = Math.Min(
            request.PageSize ?? GetNearbySchoolsRequest.DefaultPageSize,
            GetNearbySchoolsRequest.MaximumPageSize);
        var cursor = request.Cursor is null
            ? null
            : GetNearbySchoolsCursor.TryDecode(request.Cursor, out var decodedCursor)
                ? decodedCursor
                : throw new InvalidOperationException("Nearby cursor must be validated before execution.");
        var nearbySchools = await GetNearbySchoolMatchesAsync(
            validatedRequest.Latitude,
            validatedRequest.Longitude,
            validatedRequest.RadiusMiles * MetersPerMile,
            cursor,
            pageSize + 1,
            cancellationToken);

        if (nearbySchools.Count == 0)
        {
            return new SchoolsResponse
            {
                Schools = [],
                NextCursor = null,
            };
        }

        var pageSchoolMatches = nearbySchools.Count > pageSize
            ? [.. nearbySchools.Take(pageSize)]
            : nearbySchools;
        var pageSchoolIds = pageSchoolMatches
            .Select(match => match.Id)
            .ToList();
        var schools = await _context.Schools
            .AsNoTracking()
            .Where(school => pageSchoolIds.Contains(school.Id))
            .ToListAsync(cancellationToken);
        var schoolLookup = schools.ToDictionary(school => school.Id);
        var orderedSchools = pageSchoolIds
            .Select(id => SchoolResponse.FromEntity(schoolLookup[id]))
            .ToList();

        var nextCursor = nearbySchools.Count > pageSize
            ? GetNearbySchoolsCursor.Encode(new GetNearbySchoolsCursor(
                validatedRequest.Latitude,
                validatedRequest.Longitude,
                validatedRequest.RadiusMiles,
                pageSize,
                pageSchoolMatches[^1].DistanceMeters,
                pageSchoolMatches[^1].Id))
            : null;

        return new SchoolsResponse
        {
            Schools = orderedSchools,
            NextCursor = nextCursor,
        };
    }

    private static ValidatedRequest ValidateRequest(GetNearbySchoolsRequest request)
    {
        return new ValidatedRequest(
            request.Latitude ?? throw new InvalidOperationException("Nearby request must be validated before execution."),
            request.Longitude ?? throw new InvalidOperationException("Nearby request must be validated before execution."),
            request.RadiusMiles ?? throw new InvalidOperationException("Nearby request must be validated before execution."));
    }

    private async Task<List<NearbySchoolMatch>> GetNearbySchoolMatchesAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        GetNearbySchoolsCursor? cursor,
        int limit,
        CancellationToken cancellationToken)
    {
        var connection = _context.GetNpgsqlConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = NearbySearchSql;
            _ = command.Parameters.Add(new NpgsqlParameter<double>("latitude", NpgsqlDbType.Double) { TypedValue = latitude });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("longitude", NpgsqlDbType.Double) { TypedValue = longitude });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("radiusMeters", NpgsqlDbType.Double) { TypedValue = radiusMeters });
            _ = command.Parameters.Add(new NpgsqlParameter<bool>("hasCursor", NpgsqlDbType.Boolean) { TypedValue = cursor is not null });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("cursorDistanceMeters", NpgsqlDbType.Double) { TypedValue = cursor?.DistanceMeters ?? 0d });
            _ = command.Parameters.Add(new NpgsqlParameter<Guid>("cursorSchoolId", NpgsqlDbType.Uuid) { TypedValue = cursor?.SchoolId ?? Guid.Empty });
            _ = command.Parameters.Add(new NpgsqlParameter<int>("limit", NpgsqlDbType.Integer) { TypedValue = limit });

            var schoolMatches = new List<NearbySchoolMatch>(limit);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                schoolMatches.Add(new NearbySchoolMatch(
                    reader.GetGuid(0),
                    reader.GetDouble(1)));
            }

            return schoolMatches;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private sealed record NearbySchoolMatch(Guid Id, double DistanceMeters);
    private sealed record ValidatedRequest(double Latitude, double Longitude, double RadiusMiles);
}
