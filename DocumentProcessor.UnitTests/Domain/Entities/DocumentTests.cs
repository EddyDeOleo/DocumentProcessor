using DocumentProcessor.Domain.Entities;
using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.UnitTests.Domain.Entities;

public class DocumentTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var fileName = "factura_test.pdf";
        var storageUrl = "C:\\UploadedFiles\\factura_test.pdf";

        // Act
        var document = new Document(fileName, storageUrl);

        // Assert
        Assert.NotEqual(Guid.Empty, document.Id);
        Assert.Equal(fileName, document.FileName);
        Assert.Equal(storageUrl, document.StorageUrl);
        Assert.Equal(DocumentStatus.Pending, document.Status);
        Assert.True(document.UploadedAt <= DateTime.UtcNow);
        Assert.Null(document.ProcessedAt);
        Assert.Null(document.ExtractedData);
    }

    [Fact]
    public void MarkAsProcessing_ShouldUpdateStatusToProcessing()
    {
        // Arrange
        var document = new Document("test.pdf", "path/test.pdf");

        // Act
        document.MarkAsProcessing();

        // Assert
        Assert.Equal(DocumentStatus.Processing, document.Status);
        Assert.Null(document.ProcessedAt);
    }

    [Fact]
    public void MarkAsCompleted_ShouldUpdateStatusToCompletedAndSetExtractedData()
    {
        // Arrange
        var document = new Document("invoice.pdf", "path/invoice.pdf");
        var extractedData = new ExtractedData(
            document.Id,
            DocumentType.Invoice,
            "Claro Dominicana",
            "101001569",
            1500m,
            "DOP",
            DateTime.UtcNow,
            0.95,
            "{}"
        );

        // Act
        document.MarkAsCompleted(extractedData);

        // Assert
        Assert.Equal(DocumentStatus.Completed, document.Status);
        Assert.NotNull(document.ProcessedAt);
        Assert.True(document.ProcessedAt <= DateTime.UtcNow);
        Assert.NotNull(document.ExtractedData);
        Assert.Equal("Claro Dominicana", document.ExtractedData.VendorName);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusToFailedAndSetProcessedAt()
    {
        // Arrange
        var document = new Document("corrupted.pdf", "path/corrupted.pdf");

        // Act
        document.MarkAsFailed();

        // Assert
        Assert.Equal(DocumentStatus.Failed, document.Status);
        Assert.NotNull(document.ProcessedAt);
        Assert.True(document.ProcessedAt <= DateTime.UtcNow);
        Assert.Null(document.ExtractedData);
    }
}