using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using DocumentProcessor.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Application.UseCases;

public class GetExtractedDataUseCaseTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<GetExtractedDataUseCase>> _loggerMock = new();
    private readonly GetExtractedDataUseCase _useCase;

    public GetExtractedDataUseCaseTests()
    {
        _useCase = new GetExtractedDataUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DocumentWithData_ShouldReturnExtractedDataDto()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document("receipt.pdf", "path/receipt.pdf");
        var extractedData = new ExtractedData(
            documentId,
            DocumentType.Invoice,
            "Altice",
            "101001569",
            2950m,
            "DOP",
            DateTime.UtcNow,
            0.98,
            "{}"
        );

        document.MarkAsCompleted(extractedData);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Altice", result.VendorName);
        Assert.Equal(2950m, result.TotalAmount);
    }

    [Fact]
    public async Task ExecuteAsync_ExtractedDataIsNull_ShouldReturnNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document("pending.pdf", "path/pending.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.Null(result);
    }
}