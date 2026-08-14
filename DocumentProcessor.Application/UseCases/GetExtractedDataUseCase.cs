namespace DocumentProcessor.Application.UseCases;

using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class GetExtractedDataUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetExtractedDataUseCase> _logger;

    public GetExtractedDataUseCase(
        IDocumentRepository documentRepository,
        ILogger<GetExtractedDataUseCase> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<ExtractedDataDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching extracted data for Document ID: {DocumentId}", id);

        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Extracted data lookup failed. Document with ID: {DocumentId} was not found.", id);
            return null;
        }

        if (document.ExtractedData == null)
        {
            _logger.LogInformation("No extracted data present for Document ID: {DocumentId}.", id);
            return null;
        }

        return document.ExtractedData.ToDto();
    }
}