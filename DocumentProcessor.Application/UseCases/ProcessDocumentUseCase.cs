
using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Application.UseCases
{
    public class ProcessDocumentUseCase
    {
        private readonly IDocumentRepository _repository;
        private readonly IFileStorageService _storageService;
        private readonly ITextExtractorService _textExtractor;
        private readonly IOpenAiService _openAiService;
        private readonly ILogger<ProcessDocumentUseCase> _logger;

        private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg"];
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public ProcessDocumentUseCase(
            IDocumentRepository repository,
            IFileStorageService storageService,
            ITextExtractorService textExtractor,
            IOpenAiService openAiService,
            ILogger<ProcessDocumentUseCase> logger)
        {
            _repository = repository;
            _storageService = storageService;
            _textExtractor = textExtractor;
            _openAiService = openAiService;
            _logger = logger;
        }

        public async Task<UploadDocumentResponseDto> ExecuteAsync(
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            ValidateInput(fileStream, fileName);

            _logger.LogInformation("Starting document ingestion process for file: {FileName}", fileName);

            // 1. Upload to storage & initialize entity in Processing state
            var storageUrl = await _storageService.UploadFileAsync(fileStream, fileName, cancellationToken);
            var document = new Document(fileName, storageUrl);
            document.MarkAsProcessing();

            // Single initial DB Save
            await _repository.AddAsync(document, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            // 2. Execute Pipeline
            try
            {
                _logger.LogInformation("Extracting text from document ID: {DocumentId}", document.Id);
                fileStream.Position = 0;
                var extractedText = await _textExtractor.ExtractTextAsync(fileStream, cancellationToken);

                if (string.IsNullOrWhiteSpace(extractedText))
                    throw new InvalidOperationException("Text extraction yielded an empty result.");

                _logger.LogInformation("Processing text with OpenAI for Document ID: {DocumentId}", document.Id);
                var aiResult = await _openAiService.ProcessTextAsync(extractedText, cancellationToken);

                var extractedData = new ExtractedData(
                    document.Id,
                    aiResult.DocumentType,
                    aiResult.VendorName,
                    aiResult.TaxId,
                    aiResult.TotalAmount,
                    aiResult.Currency,
                    aiResult.IssueDate,
                    aiResult.ConfidenceScore,
                    aiResult.RawJsonResponse
                );

                document.MarkAsCompleted(extractedData);
                _logger.LogInformation("Document ID: {DocumentId} completed as {Type}", document.Id, aiResult.DocumentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Document ID: {DocumentId}", document.Id);
                document.MarkAsFailed();
                throw;
            }
            finally
            {
                // Single final DB Save (persists either Completed + ExtractedData OR Failed)
                await _repository.SaveChangesAsync(cancellationToken);
            }

            return new UploadDocumentResponseDto(
                document.Id,
                document.FileName,
                document.Status,
                document.UploadedAt
            );
        }

        private static void ValidateInput(Stream fileStream, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));

            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("File stream is empty or null.", nameof(fileStream));

            if (fileStream.Length > MaxFileSizeBytes)
                throw new ArgumentException($"File size exceeds maximum limit of 10 MB.");

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Unsupported file extension '{extension}'.");
        }
    }
}

