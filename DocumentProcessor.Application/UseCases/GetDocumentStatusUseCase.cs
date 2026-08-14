namespace DocumentProcessor.Application.UseCases;

using DocumentProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class GetDocumentStatusUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetDocumentStatusUseCase> _logger;

    public GetDocumentStatusUseCase(
        IDocumentRepository documentRepository,
        ILogger<GetDocumentStatusUseCase> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<string?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching status for Document ID: {DocumentId}", id);

        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Status lookup failed. Document with ID: {DocumentId} was not found.", id);
            return null;
        }

        return document.Status.ToString();
    }
}