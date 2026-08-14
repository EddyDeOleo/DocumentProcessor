using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Application.UseCases;

public class GetAllDocumentsUseCaseTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<ILogger<GetAllDocumentsUseCase>> _loggerMock = new();
    private readonly GetAllDocumentsUseCase _useCase;

    public GetAllDocumentsUseCaseTests()
    {
        _useCase = new GetAllDocumentsUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DocumentsExist_ShouldReturnMappedDtoList()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var documentList = new List<Document>
        {
            new("doc1.pdf", "path/1"),
            new("doc2.pdf", "path/2")
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentList);

        // Act
        var result = await _useCase.ExecuteAsync(pageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(r => r.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoDocuments_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;

        _repositoryMock
            .Setup(r => r.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Document>());

        // Act
        var result = await _useCase.ExecuteAsync(pageNumber, pageSize);

        // Assert
        Assert.Empty(result);
        _repositoryMock.Verify(r => r.GetAllAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }
}