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

        var handler = new SubmitPublicCsaApplicationReviewCommandHandler(CreateDbContext(), new AlwaysVerifiedAnonymousBotVerifier());

        var result = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewCommand(
                SchoolId: school.Id,
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review.",
                BotVerificationToken: null,
                RemoteIpAddress: "203.0.113.1"),
            TestContext.Current.CancellationToken);

        Assert.Equal(SubmitPublicCsaApplicationReviewExecutionStatus.Created, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal(school.Id, result.Response.SchoolId);
        Assert.Equal("Parent", result.Response.Name);
        Assert.True(result.Response.ApplicationSuccessful);
        Assert.Equal("Helpful review.", result.Response.Comment);
        Assert.Equal("visible", result.Response.Status);

        var savedReview = await CreateDbContext().CsaApplicationReviews.SingleAsync(x => x.Id == result.Response.Id, TestContext.Current.CancellationToken);
        Assert.Equal(CsaApplicationReviewStatus.Visible, savedReview.Status);
    }

    [Fact]
    public async Task GivenUnknownSchool_WhenExecuteAsync_ThenReturnsSchoolNotFound()
    {
        var handler = new SubmitPublicCsaApplicationReviewCommandHandler(CreateDbContext(), new AlwaysVerifiedAnonymousBotVerifier());

        var result = await handler.ExecuteAsync(
            new SubmitPublicCsaApplicationReviewCommand(
                SchoolId: Guid.NewGuid(),
                Name: "Parent",
                ApplicationSuccessful: true,
                Comment: "Helpful review.",
                BotVerificationToken: null,
                RemoteIpAddress: "203.0.113.1"),
            TestContext.Current.CancellationToken);

        Assert.Equal(SubmitPublicCsaApplicationReviewExecutionStatus.SchoolNotFound, result.Status);
        Assert.Null(result.Response);
    }

    private async Task SeedSchoolAsync(School school)
    {
        var dbContext = CreateDbContext();
        _ = dbContext.Schools.Add(school);
        _ = await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
