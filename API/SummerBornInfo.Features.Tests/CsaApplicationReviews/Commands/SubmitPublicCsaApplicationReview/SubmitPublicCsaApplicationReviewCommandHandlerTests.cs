namespace SummerBornInfo.Features.Tests.CsaApplicationReviews.Commands.SubmitPublicCsaApplicationReview;

public sealed class SubmitPublicCsaApplicationReviewCommandHandlerTests(
    IntegrationTestDatabaseServerFixture testDatabaseServerFixture,
    ITestOutputHelper testOutputHelper)
    : IntegrationTestBase(testDatabaseServerFixture, testOutputHelper)
{
    [Fact]
    public async Task GivenExistingSchool_WhenExecuteAsync_ThenPersistsReviewAndReturnsPublicResponse()
    {
        var school = SchoolFactory.GetSchool();
        await SeedSchoolAsync(school);

        var handler = new SubmitPublicCsaApplicationReviewCommandHandler(CreateDbContext());

        var response = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewCommand(
                SchoolId: school.Id,
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review."),
            TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(school.Id, response.SchoolId);
        Assert.Equal("Parent", response.Name);
        Assert.True(response.ApplicationSuccessful);
        Assert.Equal("Helpful review.", response.Comment);
        Assert.Equal("visible", response.Status);

        var savedReview = await CreateDbContext().CsaApplicationReviews.SingleAsync(x => x.Id == response.Id, TestContext.Current.CancellationToken);
        Assert.Equal(CsaApplicationReviewStatus.Visible, savedReview.Status);
    }

    [Fact]
    public async Task GivenUnknownSchool_WhenExecuteAsync_ThenReturnsNull()
    {
        var handler = new SubmitPublicCsaApplicationReviewCommandHandler(CreateDbContext());

        var response = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewCommand(
                SchoolId: Guid.NewGuid(),
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review."),
            TestContext.Current.CancellationToken);

        Assert.Null(response);
    }

    private async Task SeedSchoolAsync(School school)
    {
        var dbContext = CreateDbContext();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
