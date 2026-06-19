namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public sealed class GetAdminCsaApplicationReviewQueueQueryHandler(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<GetAdminCsaApplicationReviewQueueResponse> ExecuteAsync(
        GetAdminCsaApplicationReviewQueueQuery query,
        CancellationToken cancellationToken)
    {
        var pageSize = query.PageSize ?? GetAdminCsaApplicationReviewQueueQuery.DefaultPageSize;
        var normalizedQueueStates = GetAdminCsaApplicationReviewQueueQueryValidator.NormalizeQueueStates(query.QueueStates);
        var cursor = DecodeCursor(query.Cursor, normalizedQueueStates);
        var queueRows = await LoadQueueRowsAsync(query.QueueStates, cancellationToken);
        var queuePage = GetPage(queueRows, cursor, pageSize);

        var hasNextPage = queuePage.Count > pageSize;
        var page = hasNextPage
            ? [.. queuePage.Take(pageSize)]
            : queuePage;

        return new GetAdminCsaApplicationReviewQueueResponse(
            [.. page.Select(x => new GetAdminCsaApplicationReviewQueueItemResponse(
                x.Id,
                x.ReviewerName,
                x.ApplicationSuccessful,
                x.Comment,
                ToApiStatus(x.Status),
                x.SubmittedAtUtc,
                x.OpenReportCount,
                x.PostApprovalDistinctReportCount,
                x.LatestReportAtUtc,
                new AdminCsaApplicationReviewQueueSchoolResponse(
                    x.SchoolId,
                    x.SchoolUrn,
                    x.SchoolName),
                x.Reports))],
            hasNextPage
                ? AdminCsaApplicationReviewQueueCursor.Encode(
                    new AdminCsaApplicationReviewQueueCursor(
                        normalizedQueueStates,
                        pageSize,
                        page[^1].LatestReportAtUtc,
                        page[^1].Id))
                : null);
    }

    private static AdminCsaApplicationReviewQueueCursor? DecodeCursor(string? cursor, string normalizedQueueStates)
    {
        return cursor is null
            ? null
            : AdminCsaApplicationReviewQueueCursor.TryDecode(cursor, normalizedQueueStates, out var decodedCursor)
                ? decodedCursor
                : throw new InvalidOperationException("Queue cursor must be validated before execution.");
    }

    private async Task<List<QueueRow>> LoadQueueRowsAsync(
        IReadOnlyList<CsaApplicationReviewStatus> queueStates,
        CancellationToken cancellationToken)
    {
        var reviews = await _context.CsaApplicationReviews
            .AsNoTracking()
            .Include(x => x.Reports)
            .Where(x => queueStates.Contains(x.Status))
            .ToListAsync(cancellationToken);
        var schoolIds = reviews.Select(x => x.SchoolId).Distinct().ToArray();
        var schoolLookup = await _context.Schools
            .AsNoTracking()
            .Where(x => schoolIds.Contains(x.Id))
            .Select(x => new SchoolRow(x.Id, x.URN, x.Name))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return [.. reviews.Select(review => CreateQueueRow(review, schoolLookup[review.SchoolId]))];
    }

    private static List<QueueRow> GetPage(
        IEnumerable<QueueRow> queueRows,
        AdminCsaApplicationReviewQueueCursor? cursor,
        int pageSize)
    {
        return [.. queueRows
            .Where(x => cursor is null
                || x.LatestReportAtUtc < cursor.LatestReportAtUtc
                || (x.LatestReportAtUtc == cursor.LatestReportAtUtc && x.Id.CompareTo(cursor.ReviewId) < 0))
            .OrderByDescending(x => x.LatestReportAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(pageSize + 1)];
    }

    private static QueueRow CreateQueueRow(CsaApplicationReview review, SchoolRow school)
    {
        var openReports = review.Reports
            .Where(report => report.ResolvedAtUtc is null)
            .OrderByDescending(report => report.SubmittedAtUtc)
            .ThenByDescending(report => report.Id)
            .ToList();

        return new QueueRow(
            review.Id,
            review.Name,
            review.ApplicationSuccessful,
            review.Comment,
            review.Status,
            review.SubmittedAtUtc,
            review.SchoolId,
            school.Urn,
            school.Name,
            openReports.Count,
            review.Status == CsaApplicationReviewStatus.PendingReapproval
                ? openReports.Count
                : 0,
            [.. openReports.Select(report => new AdminCsaApplicationReviewQueueReportResponse(
                report.Id,
                report.Reason,
                report.Details,
                report.SubmittedAtUtc))],
            openReports.FirstOrDefault()?.SubmittedAtUtc ?? review.SubmittedAtUtc);
    }

    private static string ToApiStatus(CsaApplicationReviewStatus status)
    {
        return status switch
        {
            CsaApplicationReviewStatus.PendingApproval => "pendingApproval",
            CsaApplicationReviewStatus.PendingReapproval => "pendingReapproval",
            CsaApplicationReviewStatus.Approved => "approved",
            CsaApplicationReviewStatus.Rejected => "rejected",
            CsaApplicationReviewStatus.Visible => "visible",
            _ => throw new UnreachableException(),
        };
    }

    private sealed record QueueRow(
        Guid Id,
        string ReviewerName,
        bool ApplicationSuccessful,
        string Comment,
        CsaApplicationReviewStatus Status,
        DateTimeOffset SubmittedAtUtc,
        Guid SchoolId,
        int SchoolUrn,
        string SchoolName,
        int OpenReportCount,
        int PostApprovalDistinctReportCount,
        IReadOnlyList<AdminCsaApplicationReviewQueueReportResponse> Reports,
        DateTimeOffset LatestReportAtUtc);

    private sealed record SchoolRow(Guid Id, int Urn, string Name);
}
