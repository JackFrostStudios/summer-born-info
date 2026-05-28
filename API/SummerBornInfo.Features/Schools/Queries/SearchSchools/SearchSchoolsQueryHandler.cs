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
        "), ranked as ( " +
        "    select " +
        "        s.\"Id\", " +
        "        case " +
        "            when s.search_name_normalized = i.normalized_query then 3 " +
        "            when s.search_name_normalized like i.normalized_query || '%' then 2 " +
        "            when position(i.normalized_query in s.search_name_normalized) > 0 then 1 " +
        "            else 0 " +
        "        end as name_match_bucket, " +
        "        ts_rank_cd(s.search_vector, i.ts_query)::double precision as text_rank, " +
        "        word_similarity(i.normalized_query, s.search_name_normalized)::double precision as name_similarity, " +
        "        word_similarity(i.normalized_postcode_query, s.search_postcode_normalized)::double precision as postcode_similarity, " +
        "        word_similarity(i.normalized_query, s.search_address_normalized)::double precision as address_similarity, " +
        "        s.search_name_normalized " +
        "    from school as s " +
        "    cross join input as i " +
        "    where " +
        "        s.search_vector @@ i.ts_query " +
        "        or word_similarity(i.normalized_query, s.search_name_normalized) >= @nameSimilarityThreshold " +
        "        or word_similarity(i.normalized_query, s.search_address_normalized) >= @addressSimilarityThreshold " +
        "        or word_similarity(i.normalized_postcode_query, s.search_postcode_normalized) >= @postcodeSimilarityThreshold " +
        ") " +
        "select " +
        "    ranked.\"Id\", " +
        "    ranked.name_match_bucket, " +
        "    ranked.text_rank, " +
        "    ranked.name_similarity, " +
        "    ranked.postcode_similarity, " +
        "    ranked.address_similarity, " +
        "    ranked.search_name_normalized " +
        "from ranked " +
        "where " +
        "    not @hasCursor " +
        "    or row( " +
        "        -ranked.name_match_bucket, " +
        "        -ranked.text_rank, " +
        "        -ranked.name_similarity, " +
        "        -ranked.postcode_similarity, " +
        "        -ranked.address_similarity, " +
        "        ranked.search_name_normalized, " +
        "        ranked.\"Id\") > row( " +
        "        -@cursorNameMatchBucket, " +
        "        -@cursorTextRank, " +
        "        -@cursorNameSimilarity, " +
        "        -@cursorPostcodeSimilarity, " +
        "        -@cursorAddressSimilarity, " +
        "        @cursorSearchNameNormalized, " +
        "        @cursorSchoolId) " +
        "order by " +
        "    ranked.name_match_bucket desc, " +
        "    ranked.text_rank desc, " +
        "    ranked.name_similarity desc, " +
        "    ranked.postcode_similarity desc, " +
        "    ranked.address_similarity desc, " +
        "    ranked.search_name_normalized asc, " +
        "    ranked.\"Id\" asc " +
        "limit @limit;";

    private readonly ApplicationDbContext _context = context;

    public async Task<SchoolsResponse> ExecuteAsync(SearchSchoolsQuery request, CancellationToken cancellationToken)
    {
        var normalizedQuery = request.SearchText.Trim().ToLowerInvariant();
        var normalizedPostcodeQuery = RemoveWhitespace(normalizedQuery);
        var pageSize = request.PageSize ?? SearchSchoolsQuery.DefaultPageSize;
        var cursor = request.Cursor is null
            ? null
            : SearchSchoolsCursor.TryDecode(request.Cursor, normalizedQuery, out var decodedCursor)
                ? decodedCursor
                : throw new InvalidOperationException("Search cursor must be validated before execution.");

        var rankedSchools = await GetRankedSchoolsAsync(
            request.SearchText,
            normalizedQuery,
            normalizedPostcodeQuery,
            cursor,
            pageSize,
            cancellationToken);

        if (rankedSchools.Count == 0)
        {
            return new SchoolsResponse
            {
                Schools = [],
                NextCursor = null,
            };
        }

        var hasNextPage = rankedSchools.Count > pageSize;
        var pageSchools = hasNextPage
            ? [.. rankedSchools.Take(pageSize)]
            : rankedSchools;
        var rankedSchoolIds = pageSchools
            .Select(school => school.SchoolId)
            .ToList();

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
            NextCursor = hasNextPage
                ? SearchSchoolsCursor.Encode(pageSchools[^1].ToCursor(), normalizedQuery)
                : null,
        };
    }

    private async Task<List<RankedSchoolRow>> GetRankedSchoolsAsync(
        string queryText,
        string normalizedQuery,
        string normalizedPostcodeQuery,
        SearchSchoolsCursor? cursor,
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
            _ = command.Parameters.Add(new NpgsqlParameter<bool>("hasCursor", NpgsqlDbType.Boolean) { TypedValue = cursor is not null });
            _ = command.Parameters.Add(new NpgsqlParameter<int>("cursorNameMatchBucket", NpgsqlDbType.Integer) { TypedValue = cursor?.NameMatchBucket ?? 0 });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("cursorTextRank", NpgsqlDbType.Double) { TypedValue = cursor?.TextRank ?? 0d });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("cursorNameSimilarity", NpgsqlDbType.Double) { TypedValue = cursor?.NameSimilarity ?? 0d });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("cursorPostcodeSimilarity", NpgsqlDbType.Double) { TypedValue = cursor?.PostcodeSimilarity ?? 0d });
            _ = command.Parameters.Add(new NpgsqlParameter<double>("cursorAddressSimilarity", NpgsqlDbType.Double) { TypedValue = cursor?.AddressSimilarity ?? 0d });
            _ = command.Parameters.Add(new NpgsqlParameter<string>("cursorSearchNameNormalized", NpgsqlDbType.Text) { TypedValue = cursor?.SearchNameNormalized ?? string.Empty });
            _ = command.Parameters.Add(new NpgsqlParameter<Guid>("cursorSchoolId", NpgsqlDbType.Uuid) { TypedValue = cursor?.SchoolId ?? Guid.Empty });
            _ = command.Parameters.Add(new NpgsqlParameter<int>("limit", NpgsqlDbType.Integer) { TypedValue = pageSize + 1 });

            var rankedSchools = new List<RankedSchoolRow>(pageSize + 1);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rankedSchools.Add(new RankedSchoolRow(
                    reader.GetGuid(0),
                    reader.GetInt32(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4),
                    reader.GetDouble(5),
                    reader.GetString(6)));
            }

            return rankedSchools;
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

    private sealed record RankedSchoolRow(
        Guid SchoolId,
        int NameMatchBucket,
        double TextRank,
        double NameSimilarity,
        double PostcodeSimilarity,
        double AddressSimilarity,
        string SearchNameNormalized)
    {
        public SearchSchoolsCursor ToCursor()
        {
            return new SearchSchoolsCursor(
                NameMatchBucket,
                TextRank,
                NameSimilarity,
                PostcodeSimilarity,
                AddressSimilarity,
                SearchNameNormalized,
                SchoolId);
        }
    }
}
