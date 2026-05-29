namespace SummerBornInfo.Features.Schools.Queries.GetNearbySchools;

public sealed class GetNearbySchoolsQueryHandler(ApplicationDbContext context)
{
    private const double MetersPerMile = 1609.344d;
    private const string NearbySearchSql =
        "select s.\"Id\" " +
        "from school as s " +
        "where " +
        "    s.\"Location\" is not null " +
        "    and ST_DWithin( " +
        "        s.\"Location\", " +
        "        ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography, " +
        "        @radiusMeters) " +
        "order by " +
        "    ST_Distance( " +
        "        s.\"Location\", " +
        "        ST_SetSRID(ST_MakePoint(@longitude, @latitude), 4326)::geography) asc, " +
        "    s.\"Id\" asc " +
        "limit @limit;";

    private readonly ApplicationDbContext _context = context;

    public async Task<SchoolsResponse> ExecuteAsync(GetNearbySchoolsRequest request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(
            request.PageSize ?? GetNearbySchoolsRequest.DefaultPageSize,
            GetNearbySchoolsRequest.MaximumPageSize);
        var nearbySchoolIds = await GetNearbySchoolIdsAsync(
            request.Latitude ?? throw new InvalidOperationException("Nearby request must be validated before execution."),
            request.Longitude ?? throw new InvalidOperationException("Nearby request must be validated before execution."),
            (request.RadiusMiles ?? throw new InvalidOperationException("Nearby request must be validated before execution.")) * MetersPerMile,
            pageSize + 1,
            cancellationToken);

        if (nearbySchoolIds.Count == 0)
        {
            return SchoolsResponse.Create([], nextCursor: null);
        }

        var pageSchoolIds = nearbySchoolIds.Count > pageSize
            ? [.. nearbySchoolIds.Take(pageSize)]
            : nearbySchoolIds;
        var schools = await _context.Schools
            .AsNoTracking()
            .Where(school => pageSchoolIds.Contains(school.Id))
            .ToListAsync(cancellationToken);
        var schoolLookup = schools.ToDictionary(school => school.Id);
        var orderedSchools = pageSchoolIds
            .Select(id => SchoolResponse.FromEntity(schoolLookup[id]))
            .ToList();

        return SchoolsResponse.Create(orderedSchools, nextCursor: null);
    }

    private async Task<List<Guid>> GetNearbySchoolIdsAsync(
        double latitude,
        double longitude,
        double radiusMeters,
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
            _ = command.Parameters.Add(new NpgsqlParameter<int>("limit", NpgsqlDbType.Integer) { TypedValue = limit });

            var schoolIds = new List<Guid>(limit);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                schoolIds.Add(reader.GetGuid(0));
            }

            return schoolIds;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
