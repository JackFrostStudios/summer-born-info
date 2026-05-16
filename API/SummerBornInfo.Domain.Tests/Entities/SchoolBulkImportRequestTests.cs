using System.Reflection;

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
    public void GivenPendingRequest_WhenProcessingStarted_ThenStatusIsProcessing()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };

        // Act
        bool started = schoolBulkImportRequest.ProcessingStarted();

        // Assert
        Assert.True(started);
        Assert.Equal(SchoolBulkImportStatus.Processing, schoolBulkImportRequest.Status);
    }

    [Fact]
    public void GivenCompletedRequest_WhenProcessingStarted_ThenStatusIsUnchanged()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };
        schoolBulkImportRequest.ProcessingStarted();
        schoolBulkImportRequest.ProcessingComplete();

        // Act
        bool started = schoolBulkImportRequest.ProcessingStarted();

        // Assert
        Assert.False(started);
        Assert.Equal(SchoolBulkImportStatus.Completed, schoolBulkImportRequest.Status);
    }

    [Fact]
    public void GivenProcessedRows_WhenUpdatingProgress_ThenProcessedLineCountIncrements()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };

        // Act
        schoolBulkImportRequest.UpdateProgress(2, null);
        schoolBulkImportRequest.UpdateProgress(3, null);

        // Assert
        Assert.Equal(2, schoolBulkImportRequest.LinesProcessed);
        Assert.Empty(schoolBulkImportRequest.Failures);
    }

    [Fact]
    public void GivenRowFailure_WhenUpdatingProgress_ThenFailureIsRecorded()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };

        // Act
        schoolBulkImportRequest.UpdateProgress(3, "Missing URN");

        // Assert
        Assert.Single(schoolBulkImportRequest.Failures);
        Assert.Equal(3, schoolBulkImportRequest.Failures[0].LineNumber);
        Assert.Equal("Missing URN", schoolBulkImportRequest.Failures[0].ErrorMessage);
    }

    [Fact]
    public void GivenExistingFailureForRow_WhenUpdatingProgress_ThenErrorDetailsAreOverwritten()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };
        schoolBulkImportRequest.UpdateProgress(3, "Missing URN");

        // Act
        schoolBulkImportRequest.UpdateProgress(3, "URN must be numeric");

        // Assert
        Assert.Equal(2, schoolBulkImportRequest.LinesProcessed);
        Assert.Single(schoolBulkImportRequest.Failures);
        Assert.Equal("URN must be numeric", schoolBulkImportRequest.Failures[0].ErrorMessage);
    }

    [Fact]
    public void GivenProcessedRequestWithoutFailures_WhenProcessingCompletes_ThenStatusIsCompleted()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };
        schoolBulkImportRequest.ProcessingStarted();
        schoolBulkImportRequest.UpdateProgress(2, null);

        // Act
        schoolBulkImportRequest.ProcessingComplete();

        // Assert
        Assert.Equal(SchoolBulkImportStatus.Completed, schoolBulkImportRequest.Status);
    }

    [Fact]
    public void GivenProcessedRequestWithFailures_WhenProcessingCompletes_ThenStatusIsCompletedWithFailures()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };
        schoolBulkImportRequest.ProcessingStarted();
        schoolBulkImportRequest.UpdateProgress(3, "Missing URN");

        // Act
        schoolBulkImportRequest.ProcessingComplete();

        // Assert
        Assert.Equal(SchoolBulkImportStatus.CompletedWithFailures, schoolBulkImportRequest.Status);
    }

    [Fact]
    public void GivenProcessingRequest_WhenProcessingFails_ThenStatusIsFailed()
    {
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            ContentId = 1,
        };
        schoolBulkImportRequest.ProcessingStarted();

        // Act
        schoolBulkImportRequest.ProcessingFailed();

        // Assert
        Assert.Equal(SchoolBulkImportStatus.Failed, schoolBulkImportRequest.Status);
    }

    [Fact]
    public void SchoolBulkImportRequest_ShouldNotExposePublicSettersForMutableImportState()
    {
        // Act
        MethodInfo? linesProcessedSetter = typeof(SchoolBulkImportRequest).GetProperty(nameof(SchoolBulkImportRequest.LinesProcessed))!.SetMethod;
        MethodInfo? statusSetter = typeof(SchoolBulkImportRequest).GetProperty(nameof(SchoolBulkImportRequest.Status))!.SetMethod;
        MethodInfo? failuresSetter = typeof(SchoolBulkImportRequest).GetProperty(nameof(SchoolBulkImportRequest.Failures))!.SetMethod;

        // Assert
        Assert.False(linesProcessedSetter?.IsPublic ?? false);
        Assert.False(statusSetter?.IsPublic ?? false);
        Assert.Null(failuresSetter);
    }

    [Fact]
    public void SchoolBulkImportFailure_ShouldGenerateId()
    {
        // Arrange
        var failure = new SchoolBulkImportFailure(2, "Unable to parse row");

        // Act & Assert
        Assert.NotEqual(Guid.Empty, failure.Id);
    }
}
