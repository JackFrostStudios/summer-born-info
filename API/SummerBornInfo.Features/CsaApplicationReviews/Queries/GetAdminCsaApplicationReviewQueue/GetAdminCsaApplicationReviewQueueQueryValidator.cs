namespace SummerBornInfo.Features.CsaApplicationReviews.Queries.GetAdminCsaApplicationReviewQueue;

public static class GetAdminCsaApplicationReviewQueueQueryValidator
{
    private static readonly CsaApplicationReviewStatus[] DefaultQueueStates =
    [
        CsaApplicationReviewStatus.PendingApproval,
        CsaApplicationReviewStatus.PendingReapproval,
    ];

    public static bool TryValidate(
        string[]? queueState,
        string? cursor,
        int? pageSize,
        out GetAdminCsaApplicationReviewQueueQuery query)
    {
        query = default!;

        if (pageSize is <= 0 or > GetAdminCsaApplicationReviewQueueQuery.MaximumPageSize)
        {
            return false;
        }

        var queueStates = queueState is null || queueState.Length == 0
            ? DefaultQueueStates
            : ParseQueueStates(queueState);

        if (queueStates is null)
        {
            return false;
        }

        var normalizedQueueStates = NormalizeQueueStates(queueStates);
        if (cursor is not null
            && !AdminCsaApplicationReviewQueueCursor.TryDecode(cursor, normalizedQueueStates, out _))
        {
            return false;
        }

        query = new GetAdminCsaApplicationReviewQueueQuery(queueStates, cursor, pageSize);
        return true;
    }

    public static string NormalizeQueueStates(IReadOnlyList<CsaApplicationReviewStatus> queueStates)
    {
        return string.Join(
            ',',
            queueStates
                .Distinct()
                .OrderBy(x => x.ToString(), StringComparer.Ordinal)
                .Select(x => x.ToString()));
    }

    private static CsaApplicationReviewStatus[]? ParseQueueStates(IEnumerable<string> queueState)
    {
        var parsedStates = new List<CsaApplicationReviewStatus>();

        foreach (var rawValue in queueState)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            var segments = rawValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (!Enum.TryParse<CsaApplicationReviewStatus>(segment, ignoreCase: true, out var status)
                    || status is not CsaApplicationReviewStatus.PendingApproval and not CsaApplicationReviewStatus.PendingReapproval)
                {
                    return null;
                }

                parsedStates.Add(status);
            }
        }

        return parsedStates.Count == 0
            ? null
            : [.. parsedStates.Distinct()];
    }
}
