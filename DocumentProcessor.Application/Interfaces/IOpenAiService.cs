using DocumentProcessor.Application.DTOs;

namespace DocumentProcessor.Application.Interfaces
{
    public interface IOpenAiService
    {
        Task<ExtractedDataDto> ProcessTextAsync(string rawText, CancellationToken cancellationToken = default);
    }
}
