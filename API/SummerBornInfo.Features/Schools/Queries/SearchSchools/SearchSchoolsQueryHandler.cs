using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace SummerBornInfo.Features.Schools.Queries.SearchSchools;

public sealed class SearchSchoolsQueryHandler(ApplicationDbContext context)
{
    private const double NameSimilarityThreshold = 0.35;
    private const double AddressSimilarityThreshold = 0.35;
    private const double PostcodeSimilarityThreshold = 0.45;

    private const string RankedSearchSql =
        "with input as ( " +
        "    select " +
        "        plainto_tsquery('simple', @queryText) as ts_query, " +
        "        @normalizedQuery as normalized_query, " +
        "        @normalizedPostcodeQuery as normalized_postcode_query " +
        ") " +
        "select s.\"Id\" " +
        "from school as s " +
        "cross join input as i " +
        "where " +
        "    s.search_vector @@ i.ts_query " +
        "    or word_similarity(i.normalized_query, s.search_name_normalized) >= @nameSimilarityThreshold " +
        "    or word_similarity(i.normalized_query, s.search_address_normalized) >= @addressSimilarityThreshold " +
        "    or word_similarity(i.normalized_postcode_query, s.search_postcode_normalized) >= @postcodeSimilarityThreshold " +
        "order by " +
        "    case " +
        "        when s.search_name_normalized = i.normalized_query then 3 " +
        "        when s.search_name_normalized like i.normalized_query || '%' then 2 " +
        "        when position(i.normalized_query in s.search_name_normalized) > 0 then 1 " +
        "        else 0 " +
        "    end desc, " +
        "    ts_rank_cd(s.search_vector, i.ts_query) desc, " +
        "    word_similarity(i.normalized_query, s.search_name_normalized) desc, " +
        "    word_similarity(i.normalized_postcode_query, s.search_postcode_normalized) desc, " +
        "    word_similarity(i.normalized_query, s.search_address_normalized) desc, " +
        "    s.search_name_normalized asc, " +
        "    s.\"Id\" asc " +
        "limit @limit;";

    private readonly ApplicationDbContext _context = context;

    public async Task<SchoolsResponse> ExecuteAsync(SearchSchoolsQuery request, CancellationToken cancellationToken)
    {
        var requestedPageSize = request.PageSize ?? SearchSchoolsQuery.DefaultPageSize;
        var pageSize = Math.Min(requestedPageSize, SearchSchoolsQuery.MaximumPageSize);
        var normalizedQuery = request.SearchText.Trim().ToLowerInvariant();
        var normalizedPostcodeQuery = RemoveWhitespace(normalizedQuery);

        var rankedSchoolIds = await GetRankedSchoolIdsAsync(
            request.SearchText,
            normalizedQuery,
            normalizedPostcodeQuery,
            pageSize,
            cancellationToken);

        if (rankedSchoolIds.Count == 0)
        {
            return new SchoolsResponse
            {
                Schools = [],
                NextCursor = null,
            };
        }

        var schools = await _context.Schools
            .AsNoTracking()
            .Where(school => rankedSchoolIds.Contains(school.Id))
            .ToListAsync(cancellationToken);

        var schoolLookup = schools.ToDictionary(school => school.Id);
        var orderedSchools = rankedSchoolIds
            .Select(id => SchoolResponse.FromEntity(schoolLookup[id]))
            .ToList();

        return new SchoolsResponse
        {
            Schools = orderedSchools,
            NextCursor = null,
        };
    }

    private async Task<List<Guid>> GetRankedSchoolIdsAsync(
        string queryText,
        string normalizedQuery,
        string normalizedPostcodeQuery,
        int pageSize,
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
            command.CommandText = RankedSearchSql;
            _ = command.Parameters.Add(new NpgsqlParameter<string>("queryText", NpgsqlDbType.Text) { TypedValue = queryText });
            _ = command.Parameters.Add(new NpgsqlParameter<string>("normalizedQuery", NpgsqlDbType.Text) { TypedValue = normalizedQuery });
            _ = command.Parameters.Add(new NpgsqlParameter<string>("normalizedPostcodeQuery", NpgsqlDbType.Text) { TypedValue = normalizedPostcodeQuery });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("nameSimilarityThreshold", NpgsqlDbType.Double) { TypedValue = NameSimilarityThreshold });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("addressSimilarityThreshold", NpgsqlDbType.Double) { TypedValue = AddressSimilarityThreshold });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("postcodeSimilarityThreshold", NpgsqlDbType.Double) { TypedValue = PostcodeSimilarityThreshold });
            _ = command.Parameters.Add(new NpgsqlParameter<int>("limit", NpgsqlDbType.Integer) { TypedValue = pageSize });

            var rankedSchoolIds = new List<Guid>(pageSize);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rankedSchoolIds.Add(reader.GetGuid(0));
            }

            return rankedSchoolIds;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static string RemoveWhitespace(string value)
    {
        return string.Concat(value.Where(character => !char.IsWhiteSpace(character)));
    }
}
