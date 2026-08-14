namespace DocumentProcessor.Application.UseCases;

using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class GetDocumentByIdUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetDocumentByIdUseCase> _logger;

    public GetDocumentByIdUseCase(
        IDocumentRepository documentRepository,
        ILogger<GetDocumentByIdUseCase> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<DocumentDetailsDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching document with ID: {DocumentId}", id);

        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Document lookup failed. Document with ID: {DocumentId} was not found.", id);
            return null;
        }

        return document.ToDto();
    }
}