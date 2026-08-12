using DocumentProcessor.Application.DTOs;

namespace DocumentProcessor.Application.Interfaces
{
    public interface IAiService
    {
        Task<ExtractedDataDto> ProcessTextAsync(string rawText, CancellationToken cancellationToken = default);
    }
}
