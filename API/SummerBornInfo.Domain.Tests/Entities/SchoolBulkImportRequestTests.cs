namespace SummerBornInfo.Domain.Tests.Entities;

public sealed class SchoolBulkImportRequestTests
{
    [Fact]
    public void SchoolBulkImportRequest_ShouldInitalizeWithRequiredProperties()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };

        // Act & Assert
        Assert.Equal(1u, schoolBulkImportRequest.ContentId);
        Assert.Equal(0, schoolBulkImportRequest.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.Pending, schoolBulkImportRequest.Status);
        Assert.Empty(schoolBulkImportRequest.Failures);
    }

    [Fact]
    public void SchoolBulkImportRequest_ShouldGenerateId()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, schoolBulkImportRequest.Id);
    }

    [Fact]
    public void SchoolBulkImportRequest_ShouldInitalizeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            Id = id,
            ContentId = 1,
            LinesProcessed = 4,
            Status = SchoolBulkImportStatus.CompletedWithFailures,
            Failures =
            [
                new SchoolBulkImportFailure
                {
                    LineNumber = 3,
                    ErrorMessage = "Missing URN",
                },
            ],
        };

        // Act & Assert
        Assert.Equal(id, schoolBulkImportRequest.Id);
        Assert.Equal(1u, schoolBulkImportRequest.ContentId);
        Assert.Equal(4, schoolBulkImportRequest.LinesProcessed);
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures, schoolBulkImportRequest.Status);
        Assert.Single(schoolBulkImportRequest.Failures);
        Assert.Equal(3, schoolBulkImportRequest.Failures[0].LineNumber);
        Assert.Equal("Missing URN", schoolBulkImportRequest.Failures[0].ErrorMessage);
    }

    [Fact]
    public void SchoolBulkImportFailure_ShouldGenerateId()
    {
        // Arrange
        var failure = new SchoolBulkImportFailure
        {
            LineNumber = 2,
            ErrorMessage = "Unable to parse row",
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, failure.Id);
    }
}
