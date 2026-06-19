namespace SummerBornInfo.Features.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public sealed class SubmitPublicCsaApplicationReviewCommandHandler(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<SubmitPublicCsaApplicationReviewResponse?> ExecuteAsync(
        SubmitPublicCsaApplicationReviewCommand command,
        CancellationToken cancellationToken)
    {
        var schoolExists = await _context.Schools
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.SchoolId, cancellationToken);

        if (!schoolExists)
        {
            return null;
        }

        var review = CsaApplicationReview.Submit(
            command.SchoolId,
            command.Name,
            command.ApplicationSuccessful,
            command.Comment,
            DateTimeOffset.UtcNow);

        _ = _context.CsaApplicationReviews.Add(review);
        _ = await _context.SaveChangesAsync(cancellationToken);

        return SubmitPublicCsaApplicationReviewResponse.FromEntity(review);
    }
}
