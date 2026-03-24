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
        // Arrange
        var schoolBulkImportRequest = new SchoolBulkImportRequest
        {
            Id = id,
            ContentId = 1,
        };

        // Act & Assert
        Assert.Equal(id, schoolBulkImportRequest.Id);
        Assert.Equal(1u, schoolBulkImportRequest.ContentId);
    }
}
