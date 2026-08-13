using DocumentProcessor.Application.Dtos;
using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace DocumentProcessor.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentsController : ControllerBase
{
    private readonly ProcessDocumentUseCase _processDocumentUseCase;
    private readonly IDocumentRepository _documentRepository;

    public DocumentsController(
        ProcessDocumentUseCase processDocumentUseCase,
        IDocumentRepository documentRepository)
    {
        _processDocumentUseCase = processDocumentUseCase;
        _documentRepository = documentRepository;
    }

    // POST: api/documents/upload
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

    // GET: api/documents/getbyid/{id}
    [HttpGet("getbyid/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            return NotFound(new { Message = $"Document with ID '{id}' was not found." });
        }

        return Ok(document.ToDto());
    }

    // GET: api/documents/get-all
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(documents.ToDtoList());
    }
}