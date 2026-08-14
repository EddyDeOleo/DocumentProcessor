namespace DocumentProcessor.Application.UseCases;

using DocumentProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class DeleteDocumentUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DeleteDocumentUseCase> _logger;

    public DeleteDocumentUseCase(
        IDocumentRepository documentRepository,
        ILogger<DeleteDocumentUseCase> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete document with ID: {DocumentId}", id);

        var document = await _documentRepository.GetByIdAsync(id, cancellationToken);
        if (document == null)
        {
            _logger.LogWarning("Delete operation failed. Document with ID: {DocumentId} was not found.", id);
            return false;
        }

        await _documentRepository.DeleteAsync(document, cancellationToken);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document with ID: {DocumentId} was successfully deleted.", id);
        return true;
    }
}