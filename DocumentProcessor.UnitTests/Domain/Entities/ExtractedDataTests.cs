using DocumentProcessor.Domain.Entities;
using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.UnitTests.Domain.Entities;

public class ExtractedDataTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentType = DocumentType.Invoice;
        var vendorName = "Altice Dominicana";
        var taxId = "101001569";
        var totalAmount = 2950.00m;
        var currency = "DOP";
        var issueDate = DateTime.UtcNow;
        var confidenceScore = 0.98;
        var rawJson = "{\"vendorName\":\"Altice Dominicana\"}";

        // Act
        var extractedData = new ExtractedData(
            documentId,
            documentType,
            vendorName,
            taxId,
            totalAmount,
            currency,
            issueDate,
            confidenceScore,
            rawJson
        );

        // Assert
        Assert.NotEqual(Guid.Empty, extractedData.Id);
        Assert.Equal(documentId, extractedData.DocumentId);
        Assert.Equal(documentType, extractedData.DocumentType);
        Assert.Equal(vendorName, extractedData.VendorName);
        Assert.Equal(taxId, extractedData.TaxId);
        Assert.Equal(totalAmount, extractedData.TotalAmount);
        Assert.Equal(currency, extractedData.Currency);
        Assert.Equal(issueDate, extractedData.IssueDate);
        Assert.Equal(confidenceScore, extractedData.ConfidenceScore);
        Assert.Equal(rawJson, extractedData.RawJsonResponse);
    }
}