
namespace DocumentProcessor.Application.Interfaces
{
    public interface ITextExtractorService
    {
        Task<string> ExtractTextAsync(Stream fileStream, CancellationToken cancellationToken = default);
    }
}
