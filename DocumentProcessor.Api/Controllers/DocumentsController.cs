using DocumentProcessor.Application.Dtos;
using DocumentProcessor.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace DocumentProcessor.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class DocumentsController : ControllerBase
{
    private readonly ProcessDocumentUseCase _processDocumentUseCase;
    private readonly DeleteDocumentUseCase _deleteDocumentUseCase;
    private readonly GetDocumentStatusUseCase _getStatusUseCase;
    private readonly GetExtractedDataUseCase _getExtractedDataUseCase;
    private readonly GetDocumentByIdUseCase _getByIdUseCase;
    private readonly GetAllDocumentsUseCase _getAllUseCase;

    public DocumentsController(
        ProcessDocumentUseCase processDocumentUseCase,
        DeleteDocumentUseCase deleteDocumentUseCase,
        GetDocumentStatusUseCase getStatusUseCase,
        GetExtractedDataUseCase getExtractedDataUseCase,
        GetDocumentByIdUseCase getByIdUseCase,
        GetAllDocumentsUseCase getAllUseCase)
    {
        _processDocumentUseCase = processDocumentUseCase;
        _deleteDocumentUseCase = deleteDocumentUseCase;
        _getStatusUseCase = getStatusUseCase;
        _getExtractedDataUseCase = getExtractedDataUseCase;
        _getByIdUseCase = getByIdUseCase;
        _getAllUseCase = getAllUseCase;
    }

    // POST: api/v1/documents/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentDto request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { Message = "File is required and cannot be empty." });
        }

        await using var stream = request.File.OpenReadStream();
        var response = await _processDocumentUseCase.ExecuteAsync(stream, request.File.FileName, cancellationToken);

        return Ok(response); 
    }

    // GET: api/v1/documents
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var documents = await _getAllUseCase.ExecuteAsync(pageNumber, pageSize, cancellationToken);
        return Ok(documents); 
    }

    // GET: api/v1/documents/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        if (document == null)
            return NotFound(new { Message = $"Document with ID '{id}' was not found." });

        return Ok(document); 
    }

    // GET: api/v1/documents/{id}/status
    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        var status = await _getStatusUseCase.ExecuteAsync(id, cancellationToken);
        if (status == null)
            return NotFound(new { Message = $"Document with ID '{id}' was not found." });

        return Ok(new { Id = id, Status = status });
    }

    // GET: api/v1/documents/{id}/extracted-data
    [HttpGet("{id:guid}/extracted-data")]
    public async Task<IActionResult> GetExtractedData(Guid id, CancellationToken cancellationToken)
    {
        var extractedData = await _getExtractedDataUseCase.ExecuteAsync(id, cancellationToken);
        if (extractedData == null)
            return NotFound(new { Message = $"No extracted data found or document with ID '{id}' does not exist." });

        return Ok(extractedData); 
    }

    // DELETE: api/v1/documents/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await _deleteDocumentUseCase.ExecuteAsync(id, cancellationToken);
        if (!success)
            return NotFound(new { Message = $"Document with ID '{id}' was not found." });

        return NoContent();
    }
}