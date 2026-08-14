namespace DocumentProcessor.Application.UseCases;

using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class GetAllDocumentsUseCase
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetAllDocumentsUseCase> _logger;

    public GetAllDocumentsUseCase(
        IDocumentRepository documentRepository,
        ILogger<GetAllDocumentsUseCase> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentDetailsDto>> ExecuteAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching paginated documents. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        var documents = await _documentRepository.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return documents.ToDtoList();
    }
}