using DocumentProcessor.Api.Controllers;
using DocumentProcessor.Application.Dtos;
using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Domain.Entities;
using DocumentProcessor.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentProcessor.UnitTests.Api.Controllers;

public class DocumentsControllerTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock = new();
    private readonly Mock<IFileStorageService> _storageServiceMock = new();
    private readonly Mock<ITextExtractorService> _textExtractorMock = new();
    private readonly Mock<IAiService> _aiServiceMock = new();

    private readonly DocumentsController _controller;

    public DocumentsControllerTests()
    {
        var processUseCase = new ProcessDocumentUseCase(
            _repositoryMock.Object,
            _storageServiceMock.Object,
            _textExtractorMock.Object,
            _aiServiceMock.Object,
            Mock.Of<ILogger<ProcessDocumentUseCase>>()
        );

        var deleteUseCase = new DeleteDocumentUseCase(_repositoryMock.Object, Mock.Of<ILogger<DeleteDocumentUseCase>>());
        var getStatusUseCase = new GetDocumentStatusUseCase(_repositoryMock.Object, Mock.Of<ILogger<GetDocumentStatusUseCase>>());
        var getExtractedDataUseCase = new GetExtractedDataUseCase(_repositoryMock.Object, Mock.Of<ILogger<GetExtractedDataUseCase>>());
        var getByIdUseCase = new GetDocumentByIdUseCase(_repositoryMock.Object, Mock.Of<ILogger<GetDocumentByIdUseCase>>());
        var getAllUseCase = new GetAllDocumentsUseCase(_repositoryMock.Object, Mock.Of<ILogger<GetAllDocumentsUseCase>>());

        _controller = new DocumentsController(
            processUseCase,
            deleteUseCase,
            getStatusUseCase,
            getExtractedDataUseCase,
            getByIdUseCase,
            getAllUseCase
        );
    }

    // --- POST: api/v1/documents/upload ---

    [Fact]
    public async Task Upload_NullFile_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new UploadDocumentDto(null!);

        // Act
        var result = await _controller.Upload(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Upload_ValidFile_ShouldReturnOkResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "Dummy file content";
        var fileName = "factura.pdf";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);

        var request = new UploadDocumentDto(fileMock.Object);

        _storageServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync("C:\\path\\factura.pdf");

        _textExtractorMock
            .Setup(t => t.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Extracted Text");

        _aiServiceMock
            .Setup(a => a.ProcessTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractedDataDto(DocumentType.Invoice, "Claro", "101", 1000m, "DOP", DateTime.UtcNow, 0.9, "{}"));

        // Act
        var result = await _controller.Upload(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    // --- GET: api/v1/documents ---

    [Fact]
    public async Task GetAll_ShouldReturnOkWithListOfDocuments()
    {
        // Arrange
        var documents = new List<Document> { new("doc1.pdf", "path1"), new("doc2.pdf", "path2") };
        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.GetAll(1, 10, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedList = Assert.IsAssignableFrom<IEnumerable<DocumentDetailsDto>>(okResult.Value);
        Assert.Equal(2, returnedList.Count());
    }

    // --- GET: api/v1/documents/{id} ---

    [Fact]
    public async Task GetById_ExistingId_ShouldReturnOk()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var document = new Document("test.pdf", "path/test.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _controller.GetById(docId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var docId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.GetById(docId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // --- GET: api/v1/documents/{id}/status ---

    [Fact]
    public async Task GetStatus_ExistingId_ShouldReturnOkWithStatus()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var document = new Document("status_doc.pdf", "path/status_doc.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _controller.GetStatus(docId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    // --- GET: api/v1/documents/{id}/extracted-data ---

    [Fact]
    public async Task GetExtractedData_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var docId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.GetExtractedData(docId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // --- DELETE: api/v1/documents/{id} ---

    [Fact]
    public async Task Delete_ExistingId_ShouldReturnNoContent()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var document = new Document("to_delete.pdf", "path/to_delete.pdf");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _controller.Delete(docId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _repositoryMock.Verify(r => r.DeleteAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var docId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.Delete(docId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
