using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Application.UseCases;

public class GetDocumentStatusUseCaseTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<GetDocumentStatusUseCase>> _loggerMock = new();
    private readonly GetDocumentStatusUseCase _useCase;

    public GetDocumentStatusUseCaseTests()
    {
        _useCase = new GetDocumentStatusUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingDocument_ShouldReturnStatusString()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document("test.pdf", "path/test.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pending", result);
        _repositoryMock.Verify(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistingDocument_ShouldReturnNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.Null(result);
    }
}