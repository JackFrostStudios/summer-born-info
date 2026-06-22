namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetPublicCsaApplicationReviews;

public sealed class GetPublicCsaApplicationReviewsQueryHandler(ApplicationDbContext context)
{
    private const string PublicVisibleReviewsSql =
        "select " +
        "    r.\"Id\", " +
        "    r.\"Name\", " +
        "    r.\"ApplicationSuccessful\", " +
        "    r.\"Comment\", " +
        "    r.\"SubmittedAtUtc\" " +
        "from csa_application_review as r " +
        "where " +
        "    r.\"SchoolId\" = @schoolId " +
        "    and r.\"Status\" in ('Visible', 'Approved') " +
        "    and ( " +
        "        not @hasCursor " +
        "        or row(r.\"SubmittedAtUtc\", r.\"Id\") < row(@cursorSubmittedAtUtc, @cursorReviewId)) " +
        "order by r.\"SubmittedAtUtc\" desc, r.\"Id\" desc " +
        "limit @limit;";

    private readonly ApplicationDbContext _context = context;

    public async Task<GetPublicCsaApplicationReviewsResponse?> ExecuteAsync(
        GetPublicCsaApplicationReviewsQuery query,
        CancellationToken cancellationToken)
    {
        var schoolExists = await _context.Schools
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.SchoolId, cancellationToken);

        if (!schoolExists)
        {
            return null;
        }

        var pageSize = query.PageSize ?? GetPublicCsaApplicationReviewsQuery.DefaultPageSize;
        var cursor = query.Cursor is null
            ? null
            : PublicCsaApplicationReviewsCursor.TryDecode(query.Cursor, out var decodedCursor)
                ? decodedCursor
                : throw new InvalidOperationException("Review cursor must be validated before execution.");

        var reviews = await GetPublicReviewsAsync(query.SchoolId, cursor, pageSize, cancellationToken);
        var hasNextPage = reviews.Count > pageSize;
        var page = hasNextPage
            ? [.. reviews.Take(pageSize)]
            : reviews;

        return new GetPublicCsaApplicationReviewsResponse(
            page,
            hasNextPage
                ? PublicCsaApplicationReviewsCursor.Encode(
                    new PublicCsaApplicationReviewsCursor(
                        pageSize,
                        page[^1].SubmittedAtUtc,
                        page[^1].Id))
                : null);
    }

    private async Task<List<GetPublicCsaApplicationReviewResponse>> GetPublicReviewsAsync(
        Guid schoolId,
        PublicCsaApplicationReviewsCursor? cursor,
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
            command.CommandText = PublicVisibleReviewsSql;
            _ = command.Parameters.Add(new NpgsqlParameter<Guid>("schoolId", NpgsqlDbType.Uuid) { TypedValue = schoolId });
            _ = command.Parameters.Add(new NpgsqlParameter<bool>("hasCursor", NpgsqlDbType.Boolean) { TypedValue = cursor is not null });
            _ = command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("cursorSubmittedAtUtc", NpgsqlDbType.TimestampTz) { TypedValue = cursor?.SubmittedAtUtc ?? DateTimeOffset.MaxValue });
            _ = command.Parameters.Add(new NpgsqlParameter<Guid>("cursorReviewId", NpgsqlDbType.Uuid) { TypedValue = cursor?.ReviewId ?? Guid.Empty });
            _ = command.Parameters.Add(new NpgsqlParameter<int>("limit", NpgsqlDbType.Integer) { TypedValue = pageSize + 1 });

            var reviews = new List<GetPublicCsaApplicationReviewResponse>(pageSize + 1);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                reviews.Add(new GetPublicCsaApplicationReviewResponse(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetBoolean(2),
                    reader.GetString(3),
                    await reader.GetFieldValueAsync<DateTimeOffset>(4, cancellationToken)));
            }

            return reviews;
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
