
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Application.UseCases;

public class DeleteDocumentUseCaseTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<DeleteDocumentUseCase>> _loggerMock = new();
    private readonly DeleteDocumentUseCase _useCase;

    public DeleteDocumentUseCaseTests()
    {
        _useCase = new DeleteDocumentUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingDocument_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document("doc.pdf", "path/doc.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistingDocument_ShouldReturnFalse()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}