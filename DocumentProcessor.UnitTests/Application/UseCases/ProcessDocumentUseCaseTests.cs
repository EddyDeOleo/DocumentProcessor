using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using DocumentProcessor.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Application.UseCases;

public class ProcessDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<IFileStorageService> _storageServiceMock = new();
    private readonly Mock<ITextExtractorService> _textExtractorMock = new();
    private readonly Mock<IAiService> _aiServiceMock = new();
    private readonly Mock<ILogger<ProcessDocumentUseCase>> _loggerMock = new();

    private readonly ProcessDocumentUseCase _useCase;

    public ProcessDocumentUseCaseTests()
    {
        _useCase = new ProcessDocumentUseCase(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _textExtractorMock.Object,
            _aiServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ValidDocument_ShouldProcessAndReturnDto()
    {
        // Arrange
        var fileName = "invoice.pdf";
        using var stream = new MemoryStream("dummy file content"u8.ToArray());
        var storageUrl = "C:\\UploadedFiles\\invoice.pdf";
        var extractedText = "Sample invoice text";

        var aiResult = new ExtractedDataDto(
            DocumentType: DocumentType.Invoice,
            VendorName: "Altice",
            TaxId: "101001569",
            TotalAmount: 2950m,
            Currency: "DOP",
            IssueDate: DateTime.UtcNow,
            ConfidenceScore: 0.98,
            RawJsonResponse: "{}"
        );

        _storageServiceMock
            .Setup(s => s.UploadFileAsync(stream, fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageUrl);

        _textExtractorMock
            .Setup(t => t.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractedText);

        _aiServiceMock
            .Setup(a => a.ProcessTextAsync(extractedText, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResult);

        // Act
        var result = await _useCase.ExecuteAsync(stream, fileName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileName, result.FileName);
        Assert.Equal(DocumentStatus.Completed, result.Status);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidExtension_ShouldThrowArgumentException()
    {
        // Arrange
        var fileName = "executable.exe";
        using var stream = new MemoryStream("invalid content"u8.ToArray());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(stream, fileName));
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_TextExtractorFails_ShouldMarkAsFailedAndRethrow()
    {
        // Arrange
        var fileName = "failing_doc.pdf";
        using var stream = new MemoryStream("failing stream content"u8.ToArray());

        _storageServiceMock
            .Setup(s => s.UploadFileAsync(stream, fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync("C:\\Files\\failing_doc.pdf");

        _textExtractorMock
            .Setup(t => t.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Extraction error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(stream, fileName));

        _repositoryMock.Verify(r => r.AddAsync(It.Is<Document>(d => d.Status == DocumentStatus.Failed), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}