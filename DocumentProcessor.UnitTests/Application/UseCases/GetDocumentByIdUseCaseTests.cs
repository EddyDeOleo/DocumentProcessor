using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Application.UseCases;

public class GetDocumentByIdUseCaseTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<GetDocumentByIdUseCase>> _loggerMock = new();
    private readonly GetDocumentByIdUseCase _useCase;

    public GetDocumentByIdUseCaseTests()
    {
        _useCase = new GetDocumentByIdUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingDocument_ShouldReturnDetailsDto()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document("invoice.pdf", "path/to/invoice.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _useCase.ExecuteAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("invoice.pdf", result.FileName);
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
        _repositoryMock.Verify(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()), Times.Once);
    }
}